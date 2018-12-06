using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArrayDisplay.File;
using ArrayDisplay.net;
using ArrayDisplay.sound;

namespace ArrayDisplay.UI {
    /// <summary>
    ///     DisPlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DisPlayWindow {
        #region Field

        public static DisPlayWindow HMainWindow;
        public static int SndCoefficent = 50;
        public static int ChNum = 1; //通道数
        public static DxPlaySound Dxplaysnd;

        readonly DataFile _dataFile;
        UdpWaveData _capudp;
        public ConstUdpArg ConstUdpArg;
        public bool IsSaveFlag;
        public UdpCommand UdpCommand;

        #endregion

        #region 属性

      

        #endregion

        public DisPlayWindow() {
            InitializeComponent();  
            HMainWindow = this;
            Dataproc.PreGraphEventHandler += OnWorkGraph;  // Work波形事件处理方法连接到事件
            Dataproc.OrigGraphEventHandler += OnOrigGraph; // Orig波形事件处理方法连接到事件
            Dataproc.DelayGraphEventHandler += OnDelayWaveGraph; // Delay波形事件处理方法连接到事件
            _dataFile = new DataFile();  //本地保存文件相关
            UdpCommand = new UdpCommand(); //用于收发Udp命令
            ConstUdpArg = new ConstUdpArg(); //获取Udp指令
            led_normaldata.FalseBrush = new SolidColorBrush(Colors.Red);   //正常工作指示灯
        }
        /// <summary>
        /// UI读入延迟波形数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">数据</param>
        void OnDelayWaveGraph(object sender, float[] e) {
            if (e == null) return;
            delay_graph.Dispatcher.Invoke(() => {
                                              delay_graph.DataSource = 0;
                                              delay_graph.DataSource = e;
                                          });
        }
        /// <summary>
        /// UI读入原始数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnOrigGraph(object sender, float[] e) {
            if (e == null) return;
            orige_graph.Dispatcher.Invoke(() => {
                                              orige_graph.DataSource = 0;
                                              orige_graph.DataSource = e;
                                          });
        }



        /// <summary>
        ///     文本框只允许输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputIntegerOnly(object sender, TextChangedEventArgs e) {
            //获取文本框内容
            TextBox text_box = (TextBox) sender;
            string value = text_box.Text.Trim();
            //检查文本是否全是数字，把非数字字符过滤掉

            //更新文本框内容
            text_box.Text = value;
        }

        /// <summary>
        ///     按钮事件响应：调试窗口最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnWindowMinClick(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        ///     按钮事件响应：调试窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnWindowShutDownClick(object sender, RoutedEventArgs e) {
            Close();
        }
        /// <summary>
        /// 听音相关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSound(object sender, RoutedEventArgs e) {
            if (Dxplaysnd == null) {
                Dxplaysnd = new DxPlaySound(31250);
                var buffer = new byte[31250 * 2];
                Dxplaysnd.ReadData(buffer);
                Dxplaysnd.ReadData(buffer);
                Dxplaysnd.ReadData(buffer);
                Dataproc.SndEventHandler += OnSndData;
                btnudp.Content = "停止听音";
            }
            else {
                if (Dataproc.SndEventHandler != null) Dataproc.SndEventHandler -= OnSndData;
                Dxplaysnd.Close();
                Dxplaysnd = null;
                btnudp.Content = "开始听音";
            }
        }

        //传输声音数据用于播放
        void OnSndData(object sender, byte[] data) {
            if (Dxplaysnd != null) Dxplaysnd.ReadData(data);
        }

        //传输声音数据用于显示
        void OnWorkGraph(object sender, float[] data) {
            if (data == null) return;
            work_graph.Dispatcher.Invoke(() => {
                                             work_graph.DataSource = 0;
                                             work_graph.DataSource = data;
                                         });
        }
        /// <summary>
        ///  TextBox按键处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_KeyDown_1(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter)
                try {
                    ChNum = int.Parse(tbChNum.Text.Trim());
                    if (ChNum < 1 || ChNum > 256) {
                        ChNum = 1;
                        tbChNum.Text = "1";
                    }
                }
                catch(Exception) {
                    // ignored
                }
        }

        void SoundValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }

        //存储数据开关
        void OnCheckBox(object sender, RoutedEventArgs e) {
            if (checkBoxSaveFile.IsChecked == true) _dataFile.EnableSaveFile();
            else _dataFile.DisableSaveFile();
        }
        /// <summary>
        /// 打开多波形界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnShowStateWindow(object sender, RoutedEventArgs e) {
            StateWindow win = new StateWindow();
            win.ShowDialog();
        }

        /// <summary>
        ///     获取系统状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetState_OnClick(object sender, RoutedEventArgs e) {
//            UdpCommand.SwitchWindow(DefinedUdpArg.SwicthToStateWindow);
            UdpCommand.GetDeviceState();
        }
        /// <summary>
        /// 保存数据按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button_Click_SaveData(object sender, RoutedEventArgs e) {
            IsSaveFlag = !IsSaveFlag;
            if (IsSaveFlag) _dataFile.EnableSaveFile();
            else _dataFile.DisableSaveFile();
        }
        /// <summary>
        /// 双击开启TabItem正常工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NormalWave_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() => { WorkDataStart_OnClick(null, null); }));
        }

        /// <summary>
        ///     连接正常工作数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WorkDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                UdpCommand.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                if (!UdpWaveData.IsRunning) {
                    _capudp = new UdpWaveData(ConstUdpArg.SrcNorm_WaveIp);
                    btnudp.Content = "停止";
                }
                else {
                    _capudp.Close();
                    btnudp.Content = "启动";
                    work_graph.Dispatcher.Invoke(() => { work_graph.DataSource = 0; });
                    work_graph.Refresh();
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
                MessageBox.Show(@"网络地址错误...");
            }
        }

        /// <summary>
        ///     原始数据图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OrigDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                UdpCommand.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
                if (!UdpWaveData.IsRunning) {
                    _capudp = new UdpWaveData(ConstUdpArg.Src_OrigWaveIp);
                    origStartBtn.Content = "停止";
                }
                else {
                    _capudp.Close();
                    origStartBtn.Content = "启动";
                    orige_graph.Dispatcher.Invoke(() => { orige_graph.DataSource = 0; });
                    orige_graph.Refresh();
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
                MessageBox.Show(@"网络地址错误...");
            }
        }
        /// <summary>
        ///  开启DelayWave工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DelayDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                UdpCommand.SwitchWindow(ConstUdpArg.SwicthToDeleyWindow);
                if (!UdpWaveData.IsRunning) {
                    _capudp = new UdpWaveData(ConstUdpArg.Src_DelayWaveIp);
                    delaystatBtn.Content = "停止";
                }
                else {
                    _capudp.Close();
                    delaystatBtn.Content = "启动";
                    delay_graph.Dispatcher.Invoke(() => { delay_graph.DataSource = 0; });
                    delay_graph.Refresh();
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
                MessageBox.Show(@"网络地址错误...");
            }
        }
    }
}
