using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArrayDisplay.net;
using ArrayDisplay.sound;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace ArrayDisplay.UI {
    /// <summary>
    ///     DisPlayWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class DisPlayWindow : IDisposable {
        //界面初始化
        public DisPlayWindow() {
            InitializeComponent();

            Dataproc.PreGraphEventHandler += OnWorkGraph; // Work时域波形事件处理方法连接到事件
            Dataproc.OrigGraphEventHandler += OnOrigGraph; // Orig波形事件处理方法连接到事件
            Dataproc.DelayGraphEventHandler += OnDelayWaveGraph; // Delay波形事件处理方法连接到事件
//          Dataproc.FrapGraphEventHandler += OnFrapGraph; //Work频域波形事件处理方法
            Dataproc.EnergyArrayEventHandler += OnEnergyArrayGraph; //能量图事件处理方法
            Dataproc.FrapPointGraphEventHandler += OnFrapPointGraph; //使用新FFT频域事件处理

            hMainWindow = this;

            systemInfo = new SystemInfo();
            SelectdInfo = new OnSelectdInfo();
            dataFile = new DataFile.DataFile();
            udpCommand = new UdpCommand();
            

            #region UI绑定

            tb_state_mc_type.SetBinding(TextBox.TextProperty, new Binding("McType") {Source = systemInfo});
            tb_state_mc_id.SetBinding(TextBox.TextProperty, new Binding("McId") {Source = systemInfo});
            tb_state_mc_mac.SetBinding(TextBox.TextProperty, new Binding("McMac") {Source = systemInfo});
            tb_setting_pulse_period.SetBinding(TextBox.TextProperty, new Binding("PulsePeriod") {Source = systemInfo});
            tb_setting_pulse_delay.SetBinding(TextBox.TextProperty, new Binding("PulseDelay") {Source = systemInfo});
            tb_setting_pulse_width.SetBinding(TextBox.TextProperty, new Binding("PulseWidth") {Source = systemInfo});
            tb_setting_adc_offset.SetBinding(TextBox.TextProperty, new Binding("AdcOffset") {Source = systemInfo});
            tb_setting_adc_num.SetBinding(TextBox.TextProperty, new Binding("AdcNum") {Source = systemInfo});

            tb_origFram.SetBinding(TextBox.TextProperty, new Binding("OrigFramNums") {Source = SelectdInfo});
            tb_deleyTime.SetBinding(TextBox.TextProperty, new Binding("ChannelDelayTime") {Source = systemInfo});
            tb_deleyChannel.SetBinding(TextBox.TextProperty, new Binding("ChannelDelayNums") {Source = systemInfo});

            tb_dacLen.SetBinding(TextBox.TextProperty, new Binding("DacLenth") {Source = SelectdInfo});
            tb_dacChannel.SetBinding(TextBox.TextProperty, new Binding("DacChannel") {Source = SelectdInfo});

            tb_workChNum.SetBinding(TextBox.TextProperty, new Binding("WorkWaveChannel") {Source = SelectdInfo, Mode = BindingMode.TwoWay});

            #endregion

            OrigChannel = 0;
            DelayChannel = 0;
            OrigTiv = 0;

            Bvalues = new float[ConstUdpArg.ORIG_TIME_NUMS * ConstUdpArg.ORIG_CHANNEL_NUMS];

            timer_bvalue = new DispatcherTimer();
            timer_blist = new DispatcherTimer();
            led_normaldata.FalseBrush = new SolidColorBrush(Colors.Red); //正常工作指示灯
            isTabWorkGotFocus = true;
            //测试,关闭数据接收
//            udpCommand.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
            //isTabWorkGotFocus = true;
            //启动后等待0.5秒 自动加载一次系统状态参数
            //new Thread(LoadSystemInfo).Start();
            //Thread.Sleep(500);
            //UdpCommand.GetDeviceState();
        }

        #region  基础功能

        /// <summary>
        ///     ///文本框只允许输入数字
        ///     /// 脉冲周期/脉冲延时/脉冲宽度///
        /// </summary>
        void InputIntegerOnly(object sender, TextCompositionEventArgs e) {
            //throw new NotImplementedException();
            Regex re = new Regex("[^0-9.-]+");
            if (e != null) {
                e.Handled = re.IsMatch(e.Text);
            }
        }

        #endregion

        void SaveOrigData_OnClick(object sender, RoutedEventArgs e) {
            isorigSaveFlag = !isorigSaveFlag;
            if (isorigSaveFlag) {
                btn_origsave.Content = "正在保存";
                dataFile.EnableOrigSaveFile();
                SelectdInfo.IsSaveData = true;
            }

            else {
                btn_origsave.Content = "保存数据";
                dataFile.DisableSaveFile();
            }
        }

        #region 读取波形数据

        /// <summary>///控件读入能量图波形/// </summary>
        void OnEnergyArrayGraph(object sender, float[] e) {
            int len = ConstUdpArg.ARRAY_USED;
            var graphdata = new float[len];

            Array.Copy(e, 0, graphdata, 0, len);

            for(int i = 0; i < len; i++) {
                if (Math.Abs(graphdata[i]) < double.Epsilon) {
                    graphdata[i] = 0;
                }
                else {
//                    graphdata[i] = 20 + 20 * (float)Math.Log10(Math.Abs(graphdata[i]));
                    graphdata[i] = 50 * graphdata[i];
                }
            }
            var xnums = new int[len];
            var dataPoints1 = new Point[len];

            for(int i = 0; i < xnums.Length; i++) {
                xnums[i] = i;
            }
            for(int i = 0; i < 64; i++) {
                dataPoints1[i] = new Point(xnums[i], graphdata[i]);
            }
            graph_energyFirst.Dispatcher.Invoke(() => {
                                                    graph_energyFirst.DataSource = 0;
                                                    graph_energyFirst.DataSource = dataPoints1;
                                                });
        }

        /// <summary>///控件读入延迟波形/// </summary>
        void OnDelayWaveGraph(object sender, float[][] e) {
            if (e == null) {
                return;
            }
            delay_graph.Dispatcher.Invoke(() => {
                                              delay_graph.DataSource = 0;
                                              delay_graph.DataSource = e[DelayChannel];
                                          });
        }

        /// <summary>///控件读入原始波形/// </summary>
        void OnOrigGraph(object sender, float[][] e) {
            if (e == null) {
                return;
            }
            orige_graph.Dispatcher.Invoke(() => {
                                              orige_graph.DataSource = 0;
                                              orige_graph.DataSource = e[OrigChannel * ConstUdpArg.ORIG_CHANNEL_NUMS + OrigTiv];
                                          });
        }

        /// <summary>///控件读入工作时域波形/// </summary>
        void OnWorkGraph(object sender, float[] e) {
            if (e == null) {
                return;
            }
            graph_normalTime.Dispatcher.Invoke(() => {
                                                   graph_normalTime.DataSource = 0;
                                                   graph_normalTime.DataSource = e;
                                               });
        }

        /// <summary>///控件读入工作频率波形/// </summary>
        void OnFrapGraph(object sender, float[] e) {
            if (e == null) {
                return;
            }
            graph_normalFrequency.Dispatcher.Invoke(() => {
                                                        graph_normalFrequency.DataSource = 0;
                                                        graph_normalFrequency.DataSource = e;
                                                    });
        }

        /// <summary>///控件读入工作频率波形/// </summary>
        void OnFrapPointGraph(object sender, Point[] e) {
            graph_normalFrequency.Dispatcher.Invoke(() => {
                                                        graph_normalFrequency.DataSource = 0;
                                                        graph_normalFrequency.DataSource = e;
                                                    });
        }

        #endregion

        #region 点击响应函数

        /// <summary>
        ///     获取系统状态
        ///     一次发送9条指令,包含:设备类型,设备ID,设备MAC,ADC_Err,脉冲周期,脉冲延时,脉冲宽度,ADC偏移,ADC禁用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetState_OnClick(object sender, RoutedEventArgs e) {
            //切换到系统设置状态
            Task.Run(() => {
                         udpCommand.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
                         try {
                             //发送查询指令
                             udpCommand.GetDeviceState();
                             Thread.Sleep(1000);
                         }
                         catch(Exception exception) {
                             Console.WriteLine(exception);
                         }
                     });
        }

        /// <summary>
        ///     保存数据按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SaveData_OnClick(object sender, RoutedEventArgs e) {
            isworkSaveFlag = !isworkSaveFlag;
            if (isworkSaveFlag) {
                btnSave.Content = "开始保存";
                dataFile.EnableWorkSaveFile();
                SelectdInfo.IsSaveData = true;
            }

            else {
                btnSave.Content = "保存数据";
                dataFile.DisableSaveFile();
            }
        }

        /// <summary>
        ///     双击开启TabItem正常工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NormalWave_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() => {
                                                  WorkDataStart_OnClick(null, null);
                                              }));
        }

        /// <summary>
        ///     连接正常工作数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WorkDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                if (!UdpWaveData.isBuilded) {
                    capudp = new UdpWaveData(ConstUdpArg.SrcNorm_WaveIp);
                    btn_normalstart.Content = "停止";
                }
                else {
                    if (UdpWaveData.isBuilded && UdpWaveData.WaveType == ConstUdpArg.WaveType.Normal) {
                        capudp.Close();
                        btn_normalstart.Content = "启动";
                    }
                    else {
                        capudp.Close();
                        capudp = new UdpWaveData(ConstUdpArg.SrcNorm_WaveIp);
                        btn_normalstart.Content = "停止";
                        btn_delaystart.Content = "启动";
                        btn_origstart.Content = "启动";
                    }
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
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
                if (!UdpWaveData.isBuilded) {
                    capudp = new UdpWaveData(ConstUdpArg.Src_OrigWaveIp);
                    udpCommand.WriteOrigChannel(OrigChannel);
                    udpCommand.WriteOrigTDiv(OrigTiv);
                    btn_origstart.Content = "停止";
                }
                else {
                    if (UdpWaveData.isBuilded && UdpWaveData.WaveType == ConstUdpArg.WaveType.Orig) {
                        capudp.Close();
                        btn_origstart.Content = "启动";
                        orige_graph.Dispatcher.Invoke(() => {
                                                          orige_graph.DataSource = 0;
                                                      });
                        orige_graph.Refresh();
                    }
                    else {
                        capudp = new UdpWaveData(ConstUdpArg.Src_OrigWaveIp);
                        btn_origstart.Content = "停止";
                        btn_delaystart.Content = "启动";
                        btn_normalstart.Content = "启动";
                    }
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
                MessageBox.Show(@"网络地址错误...");
            }
        }

        /// <summary>
        ///     开启DelayWave工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DelayDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToDeleyWindow);
                if (!UdpWaveData.isBuilded) {
                    capudp = new UdpWaveData(ConstUdpArg.Src_DelayWaveIp);
                    btn_delaystart.Content = "停止";
                }
                else {
                    if (UdpWaveData.isBuilded && UdpWaveData.WaveType == ConstUdpArg.WaveType.Delay) {
                        capudp.Close();
                        btn_delaystart.Content = "启动";
                        delay_graph.Dispatcher.Invoke(() => {
                                                          delay_graph.DataSource = 0;
                                                      });
                        delay_graph.Refresh();
                    }
                    else {
                        capudp = new UdpWaveData(ConstUdpArg.Src_OrigWaveIp);
                        btn_delaystart.Content = "停止";
                        btn_normalstart.Content = "启动";
                        btn_origstart.Content = "启动";
                    }
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
                MessageBox.Show(@"网络地址错误...");
            }
        }

        void Btn_Path_OnClick(object sender, RoutedEventArgs e) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            dialog.ShowNewFolderButton = true;

            DialogResult result = dialog.ShowDialog();

            tb_filePath.Text = dialog.SelectedPath;
        }

        void SaveBvalue_OnClick(object sender, RoutedEventArgs e) {
            string filepath = tb_filePath.Text;
            string filename = "Test.txt";
            string filedata = filepath + "\\" + filename;
            using(FileStream fs = new FileStream(filedata, FileMode.Open, FileAccess.Read)) {
                using(StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312"))) {
                    string line;
                    string pattern = @"[+-]?\d+[\.]?\d*"; //包括带小数点的数字和整数

                    while((line = sr.ReadLine()) != null) {
                        Console.WriteLine(line);
                        MatchCollection matchCollection = Regex.Matches(line, pattern);
                        Match newMatch = matchCollection[1];
                        Console.WriteLine(newMatch.Value);
                        //                        foreach(Match match in matchCollection) {
                        //                            Console.WriteLine(match.Value);
                        //                        }
                    }
                }
            }
        }

        /// <summary>///计算B值 /// </summary>
        void CalBvalue_OnClick(object sender, RoutedEventArgs e) {
            Dataproc.IsBavlueReaded = true;
//            OrigDataStart_OnClick(null,null);
            Task.Run(() => {
                         for(int i = 0; i < ConstUdpArg.ORIG_CHANNEL_NUMS; i++) {
                             udpCommand.WriteOrigChannel(i);
                             for(int j = 0; j < ConstUdpArg.ORIG_TIME_NUMS; j++) {
                                 udpCommand.WriteOrigTDiv(j);
                                 Thread.Sleep(200);
                             }
                         }
                         Thread.Sleep(1000);
                         blistBox.Dispatcher.Invoke(() => {
                                                        if (true) {
                                                            blistBox.Items.Clear();
                                                            foreach(float bvalue in Bvalues) {
                                                                blistBox.Items.Add(bvalue);
                                                            }
                                                            
                                                            MessageBox.Show("B值计算成功");
                                                            IsGetBvalue = true;
                                                        }
                                                    });
                     });
        }

        /// <summary>
        ///     打开多波形界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShowStateWindow_OnClick(object sender, RoutedEventArgs e) {
            StateWindow win = new StateWindow();
            win.ShowDialog();
        }

        /// <summary>
        ///     计算C值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CalCanCvalue_OnClick(object sender, RoutedEventArgs e) {
            int daclen = 0;
            int.TryParse(tb_dacLen.Text, out daclen);
            if (daclen != 0) {
                double tmp = daclen * 0.754;
                double result = Math.Round(tmp, MidpointRounding.AwayFromZero); //实现四舍五入
                tb_dacCvalue.Text = result.ToString();
            }
        }

        /// <summary>
        ///     计算B值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetBvalue_OnClick(object sender, RoutedEventArgs e) {
            if (!timer_bvalue.IsEnabled) {
                timer_bvalue.Interval = new TimeSpan(0, 0, 1);
                Dataproc.IsBavlueReaded = true;
                timer_bvalue.Start();
                timer_bvalue.Tick += TimerTick_Bvalue;
            }
            else {
                timer_bvalue.Stop();
                Dataproc.IsBavlueReaded = false;
            }
        }

        void TimerTick_Bvalue(object sender, EventArgs e) {
            float value = Bvalues[OrigChannel * 8 + OrigTiv];
            if (Math.Abs(value) > float.Epsilon) {
                tb_bValue.Text = value.ToString();
            }
        }

        /// <summary>
        ///     发送B值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendBvalue_OnClick(object sender, RoutedEventArgs e) {
            var resultvalue = new float[Bvalues.Length];
            if (IsGetBvalue) {
                for(int i = 0; i < resultvalue.Length; i++) {
                    if (Math.Abs(Bvalues[i]) > float.Epsilon) { }
                }
            }
        }

        #endregion

        #region 控件响应函数

        /// <summary>
        ///     可调整值的TextBox上滚轮动作响应(整数)
        ///     脉冲周期/脉冲延时/脉冲宽度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextboxOnMouseWheel(object sender, MouseWheelEventArgs e) {
            // e.Delta > 0,向上滚动滚轮,文本框数字+1,最大65535
            // e.Delta < 0,向下滚动滚轮,文本框数字-1,最小0
            ChangeMouseWheel(sender, true, e.Delta > 0);
        }

        /// <summary>
        ///     可调整值的TextBox上滚轮动作响应(小数)
        ///     ADC偏移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextboxOnMouseWheel2(object sender, MouseWheelEventArgs e) {
            // e.Delta > 0,向上滚动滚轮,文本框数字+0.01,最大1
            // e.Delta < 0,向下滚动滚轮,文本框数字-0.01,最小0
            ChangeMouseWheel(sender, false, e.Delta > 0);
        }

        /// <summary>
        ///     TextBox值发生变化
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="isInt">文本框中是否是整数</param>
        /// <param name="isUp">是否向上变化</param>
        void ChangeMouseWheel(object sender, bool isInt, bool isUp) {
            TextBox tb = sender as TextBox;
            string str = tb.Text;
            if (string.IsNullOrEmpty(str)) {
                return;
            }
            if (isInt) {
                decimal d = decimal.Parse(str);
                if (isUp) {
                    if ((d + 1) > 65535) {
                        return;
                    }
                    tb.Text = (d + 1).ToString();
                }
                else {
                    if ((d - 1) < 0) {
                        return;
                    }
                    tb.Text = (d - 1).ToString();
                }
            }
            else {
                float d = float.Parse(str);
                if (isUp) {
                    if ((d + 0.01) > 1) {
                        return;
                    }
                    tb.Text = (d + 0.01).ToString("G2");
                }
                else {
                    if ((d - 0.01) < 0) {
                        return;
                    }
                    tb.Text = (d - 0.01).ToString("G2");
                }
            }
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
        ///     听音相关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSound_OnClick(object sender, RoutedEventArgs e) {
            if (dxplaysnd == null) {
                dxplaysnd = new DxPlaySound(31250);
                var buffer = new byte[31250 * 2];
                dxplaysnd.WriteOneTimData(buffer);
                dxplaysnd.WriteOneTimData(buffer);
                dxplaysnd.WriteOneTimData(buffer);
                Dataproc.SoundEventHandler += OnSndData;
                btnlisten.Content = "停止听音";
            }
            else {
                if (Dataproc.SoundEventHandler != null) {
                    Dataproc.SoundEventHandler -= OnSndData;
                }
                dxplaysnd.Close();
                dxplaysnd = null;
                btnlisten.Content = "开始听音";
            }
        }

        //传输声音数据用于播放
        void OnSndData(object sender, byte[] data) {
            if (dxplaysnd != null) {
                dxplaysnd.WriteOneTimData(data);
            }
        }

        //传输数据用于显示
        /// <summary>
        ///     工作通道改变响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TextBox_KeyDown_workChNum(object sender, KeyEventArgs e) {
            TextBox tb = sender as TextBox;
            int workchNum = 1;
            if (tb != null) {
                int.TryParse(tb.Text, out workchNum);
            }
            if (e.Key == Key.Enter) {
                try {
                    if (workchNum < 1 || workchNum > 256) {
                        tb_workChNum.Text = "1";
                        SelectdInfo.WorkWaveChannel = 1;
                    }
                    SelectdInfo.WorkWaveChannel = workchNum;
                }
                catch(Exception) {
                    // ignored
                }
            }
        }

        /// <summary>
        ///     原始通道数改变响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Tb_origChannel_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TextBox tb = sender as TextBox;
                int num = 1;
                if (tb != null) {
                    int.TryParse(tb.Text, out num);
                }
                try {
                    if (num < 1 || num > ConstUdpArg.ORIG_CHANNEL_NUMS) {
                        tb_origChannel.Text = "1";
                    }
                    OrigChannel = num - 1;
                }
                catch(Exception) {
                    // ignored
                }
                udpCommand.WriteOrigChannel(num - 1);
            }
        }

        /// <summary>
        ///     原始时分改变响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Tb_orgiTdiv_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TextBox tb = sender as TextBox;
                int num = 0;
                if (tb != null) {
                    int.TryParse(tb.Text, out num);
                }
                try {
                    if (num < 1 || num > ConstUdpArg.ORIG_TIME_NUMS) {
                        num = 1;
                        tb_orgiTdiv.Text = "1";
                    }
                    OrigTiv = num - 1;
                }
                catch(Exception) {
                    // ignored
                }

                udpCommand.WriteOrigTDiv(num - 1);
            }
        }

        void SoundValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { }

        void Tb_deleyChannel_OnKeyDown(object sender, KeyEventArgs e) {
            TextBox tb = sender as TextBox;
            int channel = 1;
            if (tb != null) {
                int.TryParse(tb.Text, out channel);
            }
            if (e.Key == Key.Enter) {
                try {
                    if (channel < 1 || channel > 8) {
                        tb_deleyChannel.Text = "1";
                    }
                    DelayChannel = channel - 1;
                }
                catch(Exception) {
                    // ignored
                }
            }
        }

        //读取B值文件中的数据
        void ReadBFile() { }

        #endregion

        #region Property

        int DelayChannel {
            get;
            set;
        }

        int OrigChannel {
            get;
            set;
        }

        public static OnSelectdInfo SelectdInfo {
            get;
            set;
        }

        public int OrigTiv {
            get;
            set;
        }

        public float[] Bvalues {
            get;
            set;
        }

        public bool IsGetBvalue {
            get;
            set;
        }

        #endregion

        #region 字段

        public static DisPlayWindow hMainWindow; //当前窗口句柄
        public static SystemInfo systemInfo; //UI绑定参数文件
        //Udp_Data _capudp;
        //string _ip;
        //int _port;
        readonly DataFile.DataFile dataFile;
        public static int sndCoefficent = 50;
        static DxPlaySound dxplaysnd; //播放声音对象
        UdpWaveData capudp; //波形数据对象
        readonly UdpCommand udpCommand;
        readonly Dataproc dataproc;
        //public ConstUdpArg ConstUdpArg;
        bool isworkSaveFlag;
        bool isorigSaveFlag;
        int origChannel;

        readonly DispatcherTimer timer_bvalue;
        readonly DispatcherTimer timer_blist;

        #endregion

        //BindingSource dtBindingSource = new BindingSource();

        #region 界面切换控制

        //标记,当前界面,延时校准
        bool isTabDelayGotFocus;
        //标记,当前界面,原始数据
        bool isTabOranGotFocus;
        //标记,当前界面,正常工作
        bool isTabWorkGotFocus;
        //标记,当前界面,自动标定
        bool isTabAutoGotFocus;
        //标记,当前界面,多通道显示
        bool isTabMultGotFocus;

        //Tab切换,延时校准
        void TabDelayOnGotFocus(object sender, RoutedEventArgs e) {
            if (!isTabDelayGotFocus) {
                //切换到'延时校准'状态
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToDeleyWindow);
                isTabDelayGotFocus = true;
            }
        }

        //Tab切换,原始数据
        void TabOranOnGotFocus(object sender, RoutedEventArgs e) {
            if (!isTabOranGotFocus) {
                //切换到'原始数据'状态
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
                isTabOranGotFocus = true;
            }
        }

        //Tab切换,正常工作
        void TabWorkOnGotFocus(object sender, RoutedEventArgs e) {
            if (!isTabWorkGotFocus) {
                //切换到'正常工作'状态
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                isTabWorkGotFocus = true;
            }
        }

        //Tab切换,自动标定
        void TabAutoOnGotFocus(object sender, RoutedEventArgs e) {
            if (!isTabAutoGotFocus) {
                //切换到'自动标定'状态
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
                isTabAutoGotFocus = true;
            }
        }

        //Tab切换,多通道显示
        void TabMultOnGotFocus(object sender, RoutedEventArgs e) {
            if (!isTabMultGotFocus) {
                //切换到'多通道显示'状态
                udpCommand.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                isTabMultGotFocus = true;
            }
        }

        /// <summary>载入系统信息 </summary>
        void LoadSystemInfo() {
            Thread.Sleep(500);
            udpCommand.GetDeviceState();
        }

        #endregion

        #region 系统信息,(读/写/存)按钮响应

        #region 脉冲周期.(读/写/存).按钮响应

        /// <summary>脉冲周期.读.按钮响应</summary>
        void OnPulsePeriodRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadPulsePeriod();
        }

        /// <summary>脉冲周期.写.按钮响应</summary>
        void OnPulsePeriodWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_period.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.WritePulsePeriod(data);
        }

        /// <summary>脉冲周期.存.按钮响应</summary>
        void OnPulsePeriodSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_period.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.SavePulsePeriod(data);
        }

        #endregion

        #region 脉冲延时.(读/写/存).按钮响应

        /// <summary>脉冲延时.读.按钮响应</summary>
        void OnPulseDelayRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadPulseDelay();
        }

        /// <summary>脉冲延时.写.按钮响应</summary>
        void OnPulseDelayWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_delay.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.WritePulseDelay(data);
        }

        /// <summary>脉冲延时.存.按钮响应</summary>
        void OnPulseDelaySave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_delay.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.SavePulseDelay(data);
        }

        #endregion

        #region 脉冲宽度.(读/写/存).按钮响应

        /// <summary>脉冲宽度.读.按钮响应</summary>
        void OnPulseWidthRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadPulseWidth();
        }

        /// <summary>脉冲宽度.写.按钮响应</summary>
        void OnPulseWidthWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_width.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.WritePulseWidth(data);
        }

        /// <summary>脉冲宽度.存.按钮响应</summary>
        void OnPulseWidthSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_pulse_width.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            int intT = int.Parse(strT);
            intT = intT / 5;
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);
            udpCommand.SavePulseWidth(data);
        }

        #endregion

        #region ADC偏移.(读/写/存).按钮响应

        /// <summary>ADC偏移.读.按钮响应</summary>
        void OnAdcOffsetRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadAdcOffset();
        }

        /// <summary>ADC偏移.写.按钮响应</summary>
        void OnAdcOffsetWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_adc_offset.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            float floatT = float.Parse(strT);
            int tmpT = (int) ((floatT + 1.2) * 128 / 1.2);

            udpCommand.WriteAdcOffset((byte) tmpT);
        }

        /// <summary>ADC偏移.存.按钮响应</summary>
        void OnAdcOffsetSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_adc_offset.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            float floatT = float.Parse(strT);
            int tmpT = (int) ((floatT + 1.2) * 128 / 1.2);
            udpCommand.SaveAdcOffset((byte) tmpT);
        }

        #endregion

        #region 延时信息.(读/写/存).按钮响应

        /// <summary>///延时信息.读.按钮响应 /// </summary>
        void OnDelayTimeRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadDelyTime();
        }

        /// <summary>///延时信息.写.按钮响应 /// </summary>
        void OnDelayTimeWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_deleyTime.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            uint t = uint.Parse(strT);
            var temp = BitConverter.GetBytes(t);
            var data = new byte[2];
            for(int i = 0; i < 2; i++) {
                data[0] = temp[1];
                data[1] = temp[0];
            }
            udpCommand.WriteDelayTime(data);
        }

        /// <summary>///延时信息.存.按钮响应 /// </summary>
        void OnDelayTimeSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_deleyTime.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            int intT = int.Parse(strT);
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);

            udpCommand.SaveDelayTime(data);
        }

        #endregion

        #region Dac信息.(读/写/存).按钮响应

        void DacLenRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommand.ReadCanChannelLen();
        }

        void DacLenWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_dacLen.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            int t = int.Parse(strT);

            udpCommand.WriteDacChannel(t);
        }

        void DacLenSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_deleyTime.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            int intT = int.Parse(strT);
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);

            udpCommand.SaveDelayTime(data);
        }

        #endregion

        #endregion

        #region IDisposable

        void Dispose(bool disposing) {
            if (disposing) {
                if (dataFile != null) {
                    dataFile.Dispose();
                }
                if (capudp != null) {
                    capudp.Dispose();
                }
                if (udpCommand != null) {
                    udpCommand.Dispose();
                }
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
