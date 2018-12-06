using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArrayDisplay.MyUserControl.DataWave {
    /// <summary>
    ///     ArrayEnergyControl.xaml 的交互逻辑
    /// </summary>
    public partial class ArrayDataWaveControl:IDisposable {
        //-------------------------------------------------------------------
        //
        //  Public
        //
        //-------------------------------------------------------------------
        int validPixelNum = 1800;

        int originalLength = 31250;
        float startTime = 0;

        readonly float endTime = 1000;
        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------
        public float[] Data {
            set {
                try {
                    Array.Copy(value, 0, data, 0, OriginalLength);
                    mEvent.Set();
                }
                catch(Exception) {
//                    App.log.Error(e.ToString());
                }
            }
        }

        /// <summary>
        ///     0表示默认，1表示放大，2表示缩小
        /// </summary>
        public int ScalerMode {
            get;
            set;
        }
        //原始一帧数据长度
        public int OriginalLength {
            get {
                return originalLength;
            }
            set {
                originalLength = value;
            }
        }
        //Ui像素长度
        public int ValidPixelNum {
            get {
                return validPixelNum;
            }
            set {
                validPixelNum = value;
            }
        }
        //时间轴开始点
        public float StartTime {
            get {
                return startTime;
            }
            set {
                startTime = value;
            }
        }
        //时间轴结束
        public float EndTime {
            get {
                return endTime;
            }
        }

        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        public ArrayDataWaveControl() {
            InitializeComponent();

            y1.IsEnabled = false;
            y3.IsEnabled = false;

            runing = true;
            hthread = new Thread(ThreadRender);
            hthread.IsBackground = true;
            hthread.Start();
        }

        #endregion

        #region Private Fields

        static readonly float[] scaler = {1.0F, 2.0F, 4.0F, 8.0F, 10.0F};
        int scalerindex;
        float zoomValue;
        int zoomCenter;
        float[] fSegment;

        readonly bool yAutoFlag = true;
        float upVal;
        float downVal = 2;

        readonly Thread hthread;
        readonly AutoResetEvent mEvent = new AutoResetEvent(false);
        bool runing;
        readonly Mutex mMutex = new Mutex();
        CutData cutData;
        float[] data;
        float fMax;
        float fMin;
        readonly Image image = new Image();

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void UserControl_Loaded_1(object sender, RoutedEventArgs e) {
            data = new float[OriginalLength];
            zoomValue = 1.0F;
            zoomCenter = 0;
            ScalerMode = 0;
            fSegment = new float[ValidPixelNum];
            data = new float[OriginalLength];
            cutData = new CutData(OriginalLength, ValidPixelNum, 0, OriginalLength - 1,
                                  OriginalLength);
            Axis.DrawxAxis(canvas11, canvas21, StartTime, EndTime, 60, cutData.DstDataLength);
            canvas01.Children.Add(image);
        }

        void UserControl_Unloaded_1(object sender, RoutedEventArgs e) {
            if (hthread != null) hthread.Abort();
            runing = false;
            mEvent.Close();
            mEvent.Dispose();
            mMutex.Close();
            mMutex.Dispose();
        }

        void ThreadRender() {
            while(runing) {
                mEvent.WaitOne();
                mMutex.WaitOne();
                cutData.GetCenterData(data, ref fSegment);
                fMax = data.Max();
                fMin = data.Min();
                fMax = fSegment.Max();
                fMin = fSegment.Min();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                       new Action(() => { RenderPath(ref fSegment); }));
                mMutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     鼠标左单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void canvas01_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(canvas01);
            if (p.X < 0 || p.X > ValidPixelNum || p.Y < 0) return;
            switch(ScalerMode) {
                case 1:
                    if (scalerindex < (scaler.Length - 1)) scalerindex++;
                    else return;
                    break;
                case 2:
                    if (scalerindex > 0) scalerindex--;
                    else return;
                    break;
                default:
                    if (e.ClickCount == 2) {
                        scalerindex = 0;
                        cutData.CutLeft = 0;
                        cutData.CutRight = OriginalLength - 1;
                        cutData.CutDataLength = OriginalLength;
                    }
                    else return;
                    break;
            }
            zoomValue = scaler[scalerindex];
            zoomCenter =
                    (int)
                    (p.X * (cutData.CutRight - cutData.CutLeft) / cutData.DstDataLength +
                     cutData.CutLeft); //计算中心点          
            mMutex.WaitOne();
            cutData.ReCutLength(zoomValue, zoomCenter);
            cutData.GetCenterData(data, ref fSegment);
            fMax = fSegment.Max();
            fMin = fSegment.Min();
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   new Action(() => { RenderPath(ref fSegment); }));

            mMutex.ReleaseMutex();
            float fmin = cutData.CutLeft * (EndTime - StartTime) / (cutData.SrcDataLength - 1) +
                         StartTime;
            float fmax = cutData.CutRight * (EndTime - StartTime) / (cutData.SrcDataLength - 1) +
                         StartTime;
            Axis.DrawxAxis(canvas11, canvas21, fmin, fmax, 60, cutData.DstDataLength);
        }

        #region 绘制曲线

        void RenderPath(ref float[] imgdata) {
            float y_max = fMax;
            float y_min = fMin;

            if (!(y_max > y_min)) return;
            CordAxis axisy = new CordAxis {
                CanvasLine = ycanvas01,
                CanvasText = ycanvas00,
                FMin = y_min,
                FMax = y_max,
                Num = 4,
                Span = 25
            };
            axisy.DrawAxis(ref y_min, ref y_max);

            WriteableBitmap wb = BitmapFactory.New(1800, 100);
            Point pstart = new Point();
            Point pend = new Point();

            float value1 = (imgdata[0] - y_min) * 100 / (y_max - y_min);

            for(int i = 1; i < imgdata.Length; i++) {
                float value2 = (imgdata[i] - y_min) * 100 / (y_max - y_min);
                pstart.X = i - 1;
                pstart.Y = value1;
                pend.X = i;
                pend.Y = value2;
                value1 = value2;
                wb.DrawLineBresenham((int) pstart.X, (int) pstart.Y, (int) pend.X, (int) pend.Y,
                                     Colors.YellowGreen);
            }
            image.Source = wb;
        }

        #endregion

        /// <summary>
        /// </summary>
        void SetYlabel(float max, float min) {
            y3.Text = string.Format("{0:F1}", min);
            y1.Text = string.Format("{0:F1}", max);
            float fy2 = min + Math.Abs(max - min) / 2;
            y2.Text = string.Format("{0:F1}", fy2);
        }

        /// <summary>
        /// </summary>
        void OnMouseMoveArrayData(object sender, MouseEventArgs e) {
            Point p = e.GetPosition(canvas01);
            if (p.X > ValidPixelNum || p.X < 0) return;
            if (cutData == null) return;
            float bi = (float) p.X * 1.0F * cutData.CutDataLength / ValidPixelNum + cutData.CutLeft;
            if ((int) bi >= data.Length) return;
            string str = string.Format("{0:F2}", fSegment[(int) p.X]);
            bi = bi * (EndTime - StartTime) / cutData.SrcDataLength + StartTime;
            textblockTime.Text = bi.ToString("F2") + "ms;" + str;
            ScaleTransform stf = new ScaleTransform();
            stf.ScaleY = -1;
            TranslateTransform ttf = new TranslateTransform();
            //ttf.X = p.X - 80;
            //ttf.Y = 100;
            float tx = (float) (p.X - textblockTime.ActualWidth / 2);

            ttf.X = tx > 0 ? tx : 0;

            ttf.Y = 120;

            TransformGroup tfg = new TransformGroup();
            tfg.Children.Add(stf);
            tfg.Children.Add(ttf);
            textblockTime.RenderTransform = tfg;
        }

        /// <summary>
        /// </summary>
        void OnMouseEnterArrayData(object sender, MouseEventArgs e) {
            textblockTime.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// </summary>
        void OnMouseLeaveArrayData(object sender, MouseEventArgs e) {
            textblockTime.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// </summary>
        void y1_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (yAutoFlag) return;

                float up, down;
                string txt = upVal.ToString(CultureInfo.CurrentCulture);
                try {
                    up = float.Parse(y1.Text.Trim());
                    down = float.Parse(y3.Text.Trim());
                    if (up > down) {
                        SetYlabel(up, down);
                        upVal = up;
                        downVal = down;
                    }
                    else y1.Text = txt;
                }
                catch(Exception) {
                    throw new Exception();
                }
            }
        }

        /// <summary>
        /// </summary>
        void y3_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (yAutoFlag) return;

                float up, down;
                string txt = downVal.ToString(CultureInfo.InvariantCulture);
                try {
                    up = float.Parse(y1.Text.Trim());
                    down = float.Parse(y3.Text.Trim());
                    if (up > down) {
                        SetYlabel(up, down);
                        upVal = up;
                        downVal = down;
                    }
                    else y3.Text = txt;
                }
                catch(Exception) {
                    throw new Exception();
                }
            }
        }

        #endregion

        //private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        //{
        //	this.PopMenu.IsOpen = false;
        //}

        //private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        //{
        //	yAutoFlag = true;
        //	if (this.y1!=null)
        //		this.y1.IsEnabled = false;
        //	if (this.y3!= null)
        //	this.y3.IsEnabled = false;
        //}

        //private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        //{
        //	yAutoFlag = false;
        //	if (this.y1 != null)
        //		this.y1.IsEnabled = true;
        //	if (this.y3 != null)
        //		this.y3.IsEnabled = true;
        //}

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (mEvent != null) mEvent.Dispose();
                if (mMutex != null) mMutex.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
