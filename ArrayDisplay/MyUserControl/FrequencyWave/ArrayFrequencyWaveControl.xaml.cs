using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArrayDisplay.MyUserControl.FrequencyWave {
    /// <summary>
    ///     UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class ArrayFrequencyWaveControl:IDisposable {
        #region 属性

        public float[] Data {
            set {
                try {
                    Array.Copy(value, 0, data, 0, data.Length);
                    mEvent.Set();
                }
                catch(Exception) {
                    // ignored
                }
            }
        }

        public int ScalerMode {
            set {
                scalerMode = value;
            }
        }

        public Image WaveImage {
            get {
                return image;
            }
        }

        #endregion

        #region Field

        public static int validPixelNum = 1800;
        public static int originalLength = 16384;
        public static float freqPrecision = 31250.0f / 32768;

        static readonly float[] scaler = new float[5];

        readonly CutData cutData;
        readonly float[] data;
        readonly Thread hthread;
        readonly Image image = new Image();
        readonly AutoResetEvent mEvent = new AutoResetEvent(false);
        readonly Mutex mMutex = new Mutex();

        readonly bool yAutoFlag = true;
        float downVal = 2;
        float fmax;
        float fmin;

        float[] fSegment;
        bool runing;
        int scalerindex;
        int scalerMode;
        float upVal;
        int zoomCenter;
        float zoomValue;

        #endregion

        /// <summary>
        /// </summary>
        public ArrayFrequencyWaveControl() {
            InitializeComponent();

            y1.IsEnabled = false;
            y3.IsEnabled = false;

            zoomValue = 1.0F;
            zoomCenter = 0;
            scalerMode = 0;

            float maxScaler = originalLength * 1.0f / validPixelNum;
            scaler[0] = 1.0F;
            scaler[4] = maxScaler;
            for(int i = 1; i < 4; i++) scaler[i] = (scaler[4] - scaler[0]) * i / 4 + scaler[0];

            data = new float[originalLength];
            fSegment = new float[validPixelNum];

            cutData = new CutData(originalLength, validPixelNum, 0, originalLength - 1, originalLength);
            canvas01.Children.Add(WaveImage);
            runing = true;
            hthread = new Thread(ThreadRender);
            hthread.IsBackground = true;
            hthread.Start();
        }

        void UserControl_Loaded_1(object sender, RoutedEventArgs e) {
            int max = (int) (freqPrecision * originalLength);
            Axis.DrawxAxis(canvas11, canvas21, 0, max, 60, validPixelNum);
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
                //fmax = this.data.Max();
                //fmin = this.data.Min();
                //cutData.GetMaxAverageData(this.data,ref this.fSegment,ref this.fSegmentAverage);
                cutData.GetMaxData(data, ref fSegment);
                fmax = fSegment.Max();
                fmin = fSegment.Min();

                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                                                                 RenderPath2(ref fSegment);
                                                                             }));
                mMutex.ReleaseMutex();
            }
        }

        /// <summary>
        ///     处理鼠标事件
        /// </summary>
        void canvas01_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(canvas01);

            if (p.X < 0 || p.X > validPixelNum || p.Y < 0) return;
            if (scalerMode == 1)
                if (scalerindex < (scaler.Length - 1)) scalerindex++;
                else return;
            else if (scalerMode == 2)
                if (scalerindex > 0) scalerindex--;
                else return;
            else {
                if (e.ClickCount == 2) scalerindex = 0;
                else return;
            }
            zoomValue = scaler[scalerindex];
            zoomCenter = (int) (p.X * (cutData.CutRight - cutData.CutLeft) / validPixelNum + cutData.CutLeft);
            //计算中心点 
            mMutex.WaitOne();
            cutData.ReCutLength(zoomValue, zoomCenter);
            //cutData.GetMaxAverageData(this.data, ref this.fSegment, ref this.fSegmentAverage);
            cutData.GetMaxData(data, ref fSegment);
            fmax = fSegment.Max();
            fmin = fSegment.Min();
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                                                             //RenderPath(fSegment);
                                                                             RenderPath2(ref fSegment);
                                                                         }));
            mMutex.ReleaseMutex();
            int min = (int) (cutData.CutLeft * freqPrecision * originalLength / (cutData.SrcDataLength - 1));
            int max = (int) (cutData.CutRight * freqPrecision * originalLength / (cutData.SrcDataLength - 1));
            Axis.DrawxAxis(canvas11, canvas21, min, max, 60, validPixelNum);
        }

        /// <summary>
        /// </summary>
        void OnMouseMoveArrayFrequency(object sender, MouseEventArgs e) {
            Point p = e.GetPosition(canvas01);
            if ((int) p.X < validPixelNum && (int) p.X >= 0) {
                float bi = (float) (p.X * cutData.CutDataLength / validPixelNum + cutData.CutLeft);
                bi = bi * freqPrecision;
                textblockHz.Text = string.Format("{0}Hz;{1:f2}dB", bi.ToString("F0"), fSegment[(int) p.X]);
                ScaleTransform stf = new ScaleTransform();
                stf.ScaleY = -1;
                TranslateTransform ttf = new TranslateTransform();
                float tx = (float) (p.X - textblockHz.ActualWidth / 2);

                if (tx > 0) ttf.X = tx;
                else ttf.X = 0;

                ttf.Y = 120;
                TransformGroup tfg = new TransformGroup();
                tfg.Children.Add(stf);
                tfg.Children.Add(ttf);
                textblockHz.RenderTransform = tfg;
            }
        }

        /// <summary>
        /// </summary>
        void OnMouseEnterArrayData(object sender, MouseEventArgs e) {
            textblockHz.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// </summary>
        void OnMouseLeaveArrayData(object sender, MouseEventArgs e) {
            textblockHz.Visibility = Visibility.Hidden;
        }

        //private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        //{
        //	this.PopMenu.IsOpen = false;
        //}

        void y3_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (yAutoFlag) return;

                float up, down;
                string txt = upVal.ToString();
                try {
                    up = float.Parse(y3.Text.Trim());
                    down = float.Parse(y1.Text.Trim());
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

        void y1_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (yAutoFlag) return;

                float up, down;
                string txt = downVal.ToString();
                try {
                    up = float.Parse(y3.Text.Trim());
                    down = float.Parse(y1.Text.Trim());
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
        void SetYlabel(float max, float min) {
            y1.Text = string.Format("{0:F2}", min);
            y3.Text = string.Format("{0:F2}", max);
            float fy2 = min + Math.Abs(max - min) / 2;
            y2.Text = string.Format("{0:F2}", fy2);
        }

        #region 绘图

/*
        /// <summary>
        ///     绘制最大值数据组波形
        /// </summary>
        private void RenderPath(float[] data) {
            float value;
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //fmax = this.data.Max();
            //fmin = this.data.Min();
            SetYlabel(_fmax, _fmin);
            if (_fmax > _fmin) {
                var pf = new PathFigure();
                value = (data[0] - _fmin) * 100 / (_fmax - _fmin);
                pf.StartPoint = new Point(0, value);
                for (var i = 1; i < ValidPixelNum; i++) {
                    value = (int) ((data[i] - _fmin) * 100 / (_fmax - _fmin));
                    var ls = new LineSegment();
                    ls.Point = new Point(i, value);
                    pf.Segments.Add(ls);
                }
                var pg = new PathGeometry();
                pg.Figures.Add(pf);
                linePath.Data = pg;
            }
            //watch.Stop();
            //Console.WriteLine("通道频域波形  {0}",watch.ElapsedMilliseconds);
        }
*/

        /// <summary>
        ///     绘制平均值数据组波形
        /// </summary>
        //private void RenderPath2(float[] data)
        //{
        //	float fmax, fmin;
        //	float value;

        //	fmax = this.data.Max();
        //	fmin = this.data.Min();
        //	if (fmax > fmin)
        //	{
        //		PathFigure pf = new PathFigure();
        //		value = (data[0] - fmin) * 100 / (fmax - fmin);
        //		pf.StartPoint = new Point(0, value);
        //		for (int i = 1; i < ValidPixelNum; i++)
        //		{
        //			value = (int)((data[i] - fmin) * 100 / (fmax - fmin));
        //			LineSegment ls = new LineSegment();
        //			ls.Point = new Point(i, value);
        //			pf.Segments.Add(ls);
        //		}

        //		List<PathFigure> lstf = new List<PathFigure>();
        //		lstf.Add(pf);
        //		PathGeometry pg = new PathGeometry(lstf);
        //		this.AveragelinePath.Data = pg;
        //	}
        //}
        void RenderPath2(ref float[] src_data) {
            float fmax = this.fmax;
            float fmin = this.fmin;

            if (fmax > fmin) {
                double min = fmin;
                double max = fmax;
                DynamicAxis2 dynamicAxis = new DynamicAxis2 {Pixles = 100, CanvasLine = canvas00, CanvasText = ycanvas00};
                dynamicAxis.Regulate(ref min, ref max, 4);
                fmin = (float) min;
                fmax = (float) max;

                WriteableBitmap wb = BitmapFactory.New(1800, 100);
                Point pstart = new Point();
                Point pend = new Point();

                float value1 = (src_data[0] - fmin) * 100 / (fmax - fmin);

                for(int i = 1; i < src_data.Length; i++) {
                    float value2 = (src_data[i] - fmin) * 100 / (fmax - fmin);
                    pstart.X = i - 1;
                    pstart.Y = value1;
                    pend.X = i;
                    pend.Y = value2;
                    value1 = value2;
                    wb.DrawLineBresenham((int) pstart.X, (int) pstart.Y, (int) pend.X, (int) pend.Y, Colors.Lime);
                }
                WaveImage.Source = wb;
            }
        }

        #endregion

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
