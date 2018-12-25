using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using ArrayDisplay.DataFile;
using ArrayDisplay.MyUserControl.ArrayEnergy;
using ArrayDisplay.net;

namespace ArrayDisplay.UI {
    /// <summary>
    ///     StateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StateWindow:IDisposable {
        static StateWindow stateWin;

        //        public UI.WindowSwitch wndSwitch;
        //默认观察通道1和通道2
        public static uint ChannelOne = 1;
        public static uint ChannelTwo = 2;
        readonly Timer _textChangeTimer;
        bool _bNum1;
        bool _bNum2;
        public StateWindow() {
            InitializeComponent();
            textNum1.Text = ChannelOne.ToString();
            textNum1.Text = ChannelOne.ToString();

            _textChangeTimer = new Timer(1000);
            _textChangeTimer.Elapsed += OnTimedEvent;
            _textChangeTimer.AutoReset = false;
            _textChangeTimer.Enabled = false;

            ArrayEnergyControl.array_Num = ConstUdpArg.ARRAY_NUM;
            ArrayEnergyControl.max_PixLen = ConstUdpArg.MAX_ENERGY_PIXELS_LENGTH;
            ArrayEnergyControl.min_PixLen = ConstUdpArg.MIN_ENERGY_PIXELS_LENGTH;
            ArrayEnergyControl.deafult_PixLen = ConstUdpArg.DEAFULT_ENERGY_PIXELS_LENGTH;

            channelTime1.OriginalLength = ConstUdpArg.WORK_FRAME_NUMS / 2;
            channelTime2.OriginalLength = ConstUdpArg.WORK_FRAME_NUMS / 2;
            energyImage.InitArrayEnergy();
            Dataproc.PreGraphEventHandler += OnTimeWavePreGraph;
            Dataproc.BckGraphEventHandler += OnTimeWaveBckGraph;
            Dataproc.EnergyArrayEventHandler += OnEnergyArrayGraph;
            Dataproc.FrapGraphEventHandler += OnFrapGraph;

        }

        void OnFrapGraph(object sender, float[] e) {
            if (e == null) { return; }
            frequencyWaveControl_one.Data = e; 
        }

        void OnEnergyArrayGraph(object sender, float[] e) {
            if (e == null) { return; }
            energyImage.Data = e;
        }

        void OnTimeWaveBckGraph(object sender, float[] e) {
            if (e==null) { return; }
            channelTime2.Data = e;
        }

        void OnTimeWavePreGraph(object sender, float[] e) {
            if (e == null) { return; }
            channelTime1.Data = e;
        }

        #region 后端事件

        /// <summary>
        ///     定时器回调函数
        /// </summary>
        void OnTimedEvent(object sender, ElapsedEventArgs e) {
            _textChangeTimer.Enabled = false;
            if (_bNum2) {
                _bNum2 = false;
                textNum2.Dispatcher.BeginInvoke(new DelegateHandlerChannelChange(TextNum2ChangeHandler));
            }
            if (_bNum1) {
                _bNum1 = false;
                textNum1.Dispatcher.BeginInvoke(new DelegateHandlerChannelChange(TextNum1ChangeHandler));
            }
        }

        void TextNum2ChangeHandler() {
            try {
                uint channel = uint.Parse(textNum2.Text);
                if (channel < 1 || channel > ConstUdpArg.ARRAY_NUM) {
                    MessageBox.Show("超出范围");
                    textNum2.Text = "2";
                }
                else ChannelTwo = channel;
//                else {
//                    _num2 = (uint) ch;
//                    unsafe {
//                        fixed (stWavformChannel* p = &TcpHost.sendCmd.WavformChannel) {
//                            p->uSCN_ChannelNumber[0] = _num1;
//                            p->uSCN_ChannelNumber[1] = _num2;
//                        }
//                    }
//                    TcpHost.bUpdateFlag[(int) AllDataUpdateFlag.FlagWavformChannel] = true;
//                }
            }
            catch (Exception) {
                MessageBox.Show("非法数字");
                textNum2.Text = "2";
            }
        }

        /// <summary>
        ///     委托处理通道编号更改
        /// </summary>
        void TextNum1ChangeHandler() {
            try {
                uint ch = uint.Parse(textNum1.Text);
                if (ch < 1 || ch > ConstUdpArg.ARRAY_NUM) {
                    MessageBox.Show("超出范围");
                    textNum1.Text = "1";
                }
                else ChannelOne = ch;
//                else {
//                    _num1 = (uint) ch;
//                    unsafe {
//                        fixed (stWavformChannel* p = &TcpHost.sendCmd.WavformChannel) {
//                            p->uSCN_ChannelNumber[0] = _num1;
//                            p->uSCN_ChannelNumber[1] = _num2;
//                        }
//                    }
//                    TcpHost.bUpdateFlag[(int) AllDataUpdateFlag.FlagWavformChannel] = true;
//                }
            }
            catch (Exception) {
                MessageBox.Show("非法数字");
                textNum1.Text = "1";
            }
        }

        /// <summary>
        ///     委托处理通道编号更改
        /// </summary>
        delegate void DelegateHandlerChannelChange();

        #endregion

        #region 前端事件

        /// <summary>
        ///     窗口载入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MianWindow_Loaded(object sender, RoutedEventArgs e) {
            stateWin = this;
            stateWin.WindowState = WindowState.Maximized;
//            DataProcess.DataProcess.ArrayTimeWaveEvent += OnArrayTimeWave;
//            DataProcess.DataProcess.ArrayFrequencyWaveEvent += OnArrayFrequencyWave;
//            DataProcess.DataProcess.ArrayEnergyDataEvent += OnArrayEnergy;
        }

        #region 窗口与界面控制

//        /// <summary>
//        ///     拖拽窗口
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        void DragWindow(object sender, MouseButtonEventArgs e) {
//            if (e.Source is StatusBarItem) DragMove();
//        }

        /// <summary>
        ///     窗口状态切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void StatusBar_MouseDoubleClick_SwitchWindowState(object sender, MouseButtonEventArgs e) {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        ///     普通Cusor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnMouseNormal_Click(object sender, RoutedEventArgs e) {
            Cursor = Cursors.Arrow;
            channelTime1.ScalerMode = 0;
//            channelFrequency1.ScalerMode = 0;
            channelTime2.ScalerMode = 0;
            channelFrequency2.ScalerMode = 0;
        }

        /// <summary>
        ///     获得放大Cusor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnZoomIn_Click(object sender, RoutedEventArgs e) {
            RelativeDirectory rd = new RelativeDirectory();
            rd.Up(2);
            string cursorpath = rd.Path + "\\Resource\\放大2.cur";
            Cursor = new Cursor(cursorpath);

            channelTime1.ScalerMode = 1;
//            channelFrequency1.ScalerMode = 1;
            channelTime2.ScalerMode = 1;
            channelFrequency2.ScalerMode = 1;
        }

        /// <summary>
        ///     获得缩小Cusor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnZoomOut_Click(object sender, RoutedEventArgs e) {
            RelativeDirectory rd = new RelativeDirectory();
            rd.Up(2);
            string cursorpath = rd.Path + "\\Resource\\缩小2.cur";
            Cursor = new Cursor(cursorpath);

            channelTime1.ScalerMode = 2;
//            channelFrequency1.ScalerMode = 2;
            channelTime2.ScalerMode = 2;
            channelFrequency2.ScalerMode = 2;
        }

        void btnSwitchWindows_Click(object sender, RoutedEventArgs e) { }

        /// <summary>
        ///     最小化窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnMinWindows_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        ///     关闭窗口
        ///     1.确认是否关闭窗口
        ///     2.取消事件订阅
        ///     3.关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnExitWindows_Click(object sender, RoutedEventArgs e) {
            string message = "关闭该窗口吗?";
            string caption = "提示";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result =
                MessageBox.Show(message, caption, buttons, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) Close();
        }

        #endregion

        #region 通道切换

        /// <summary>
        ///     输入通道编号时的处理，每次输入重启定时器，当触发定时认为输入完成，把通道号发送出去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextNum1_KeyUp(object sender, KeyEventArgs e) {
            _textChangeTimer.Enabled = true;
            _textChangeTimer.Interval = 1500;
            _bNum1 = true;
        }

        /// <summary>
        ///     输入通道编号时的处理，每次输入重启定时器，当触发定时认为输入完成，把通道号发送出去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextNum2_KeyUp(object sender, KeyEventArgs e) {
            _textChangeTimer.Enabled = true;
            _textChangeTimer.Interval = 1500;
            _bNum2 = true;
        }

        /// <summary>
        ///     时域前一个通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumGoForward1(object sender, RoutedEventArgs e) {
            if (ChannelOne > 1) {
                ChannelOne--;
                textNum1.Text = ChannelOne.ToString();
//                Dataproc.IsChannelSwitch = true;
            }
        }

        /// <summary>
        ///     时域后一个通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumGoBack1(object sender, RoutedEventArgs e) {
            if (ChannelOne < ConstUdpArg.ARRAY_NUM) {
                ChannelOne++;
                textNum1.Text = ChannelOne.ToString();
            }
        }

        /// <summary>
        ///     频域前一个通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumGoForward2(object sender, RoutedEventArgs e) {
            if (ChannelTwo > 1) {
                ChannelTwo--;

                textNum2.Text = ChannelTwo.ToString();
            }
        }

        /// <summary>
        ///     频域后一个通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NumGoBack2(object sender, RoutedEventArgs e) {
            if (ChannelTwo < ConstUdpArg.ARRAY_NUM) {
                ChannelTwo++;
                textNum2.Text = ChannelTwo.ToString();
            }
        }

        #endregion

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (_textChangeTimer != null) _textChangeTimer.Dispose();
                if (energyImage != null) energyImage.Dispose();
                if (channelTime1 != null) channelTime1.Dispose();
                if (frequencyWaveControl_one != null) frequencyWaveControl_one.Dispose();
                if (channelTime2 != null) channelTime2.Dispose();
                if (channelFrequency2 != null) channelFrequency2.Dispose();
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
