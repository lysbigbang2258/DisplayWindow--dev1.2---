using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArrayDisplay.net;
using ArrayDisplay.sound;
using NationalInstruments.Restricted;
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
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose() {
            if (dataFile != null) {
                dataFile.Dispose();
            }
            if (udpCommandSocket != null) {
                udpCommandSocket.Dispose();
            }
            if (dataproc != null) {
                dataproc.Dispose();
            }
            if (NormWaveData != null) {
                NormWaveData.Dispose();
            }
            if (OrigWaveData != null) {
                OrigWaveData.Dispose();
            }
            if (DelayWaveData != null) {
                DelayWaveData.Dispose();
            }
        }

        #endregion

        /// <summary>
        ///     界面载入后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DisPlayWindow_OnLoaded(object sender, RoutedEventArgs e) {
            Dataproc.PreGraphEventHandler += OnWorkGraph; // Work时域波形事件处理方法连接到事件
            Dataproc.PreGraphEventHandler += OnMaxWorkValue; // Work时域波形最大值
            Dataproc.OrigGraphEventHandler += OnOrigGraph; // Orig波形事件处理方法连接到事件
            Dataproc.DelayGraphEventHandler += OnDelayWaveGraph; // Delay波形事件处理方法连接到事件
            //          Dataproc.FrapGraphEventHandler += OnFrapGraph; //Work频域波形事件处理方法
            Dataproc.EnergyArrayEventHandler += OnEnergyArrayGraph; //能量图事件处理方法
            Dataproc.FrapPointGraphEventHandler += OnFrapPointGraph; //使用新FFT频域事件处理
            Dataproc.FrapPointGraphEventHandler += OnMaxFrapPoint; //频率最大值
            hMainWindow = this;

            systemInfo = new SystemInfo();
            selectdInfo = new OnSelectdInfo();
            dataFile = new DiscFile.DataFile();
            udpCommandSocket = new UdpCommandSocket();
            Load_SystemMessage();

            observableCollection = new ObservableCollection<UIBValue>();
            blistview.ItemsSource = observableCollection;

            #region UI绑定

            tb_state_mc_type.SetBinding(TextBox.TextProperty, new Binding("McType") {Source = systemInfo});
            tb_state_mc_id.SetBinding(TextBox.TextProperty, new Binding("McId") {Source = systemInfo});
            tb_state_mc_mac.SetBinding(TextBox.TextProperty, new Binding("McMac") {Source = systemInfo});
            tb_setting_pulse_period.SetBinding(TextBox.TextProperty, new Binding("PulsePeriod") {Source = systemInfo});
            tb_setting_pulse_delay.SetBinding(TextBox.TextProperty, new Binding("PulseDelay") {Source = systemInfo});
            tb_setting_pulse_width.SetBinding(TextBox.TextProperty, new Binding("PulseWidth") {Source = systemInfo});
            tb_setting_adc_offset.SetBinding(TextBox.TextProperty, new Binding("AdcOffset") {Source = systemInfo});
            tb_setting_adc_num.SetBinding(TextBox.TextProperty, new Binding("AdcNum") {Source = systemInfo});
            tb_origFram.SetBinding(TextBox.TextProperty, new Binding("OrigFramNums") {Source = selectdInfo});
            tb_deleyTime.SetBinding(TextBox.TextProperty, new Binding("ChannelDelayTime") {Source = systemInfo});
            tb_deleyChannel.SetBinding(TextBox.TextProperty, new Binding("ChannelDelayNums") {Source = systemInfo});
            tb_dacLen.SetBinding(TextBox.TextProperty, new Binding("DacLenth") {Source = selectdInfo});
            tb_dacChannel.SetBinding(TextBox.TextProperty, new Binding("DacChannel") {Source = selectdInfo});
            tb_workChNum.SetBinding(TextBox.TextProperty, new Binding("WorkWaveChannel") {Source = selectdInfo, Mode = BindingMode.TwoWay});

            #endregion

            OrigChannel = 0;
            DelayChannel = 0;
            OrigTiv = 0;
            IsGraphPause = false;
            MainStopwatch = new Stopwatch();
            timer_bvalue = new DispatcherTimer();
            timer_blist = new DispatcherTimer();
            led_normaldata.FalseBrush = new SolidColorBrush(Colors.Red); //正常工作指示灯
        }

        /// <summary>
        ///     运行时加载系统信息，如果网络连接失败，报错
        /// </summary>
        void Load_SystemMessage() {
            udpCommandSocket.TestConnect();
            if (!udpCommandSocket.IsSocketConnect) {
                MessageBox.Show("网络连接错误");
            }
            else {
                //切换到系统设置状态
                Task.Run(() => {
                             udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
                             try {
                                 //发送查询指令
                                 udpCommandSocket.GetDeviceState();
                             }
                             catch(Exception exception) {
                                 Console.WriteLine(exception);
                             }
                         });
            }
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
                selectdInfo.IsSaveData = true;
            }

            else {
                btn_origsave.Content = "保存数据";
                dataFile.DisableSaveFile();
            }
        }

        /// <summary>
        ///     更改原始通道的时分/通道参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChangeOrigConfig_OnClick(object sender, RoutedEventArgs e) {
            int ch = 1;
            int tdiv = 1;
            int.TryParse(tb_origChannel.Text, out ch);
            int.TryParse(tb_orgiTdiv.Text, out tdiv);

            try {
                if (ch < 1 || ch > ConstUdpArg.ORIG_CHANNEL_NUMS)
                {
                    MessageBox.Show("请输入正确的通道值");
                    ch = 1;
                }
                if (tdiv < 1 || tdiv > ConstUdpArg.ORIG_TIME_NUMS)
                {
                    MessageBox.Show("请输入正确的时分值");
                    ch = 1;
                }
                OrigChannel = ch - 1;
                OrigTiv = tdiv - 1;
            }
            catch(Exception) {
                // ignored
            }
            udpCommandSocket.WriteOrigChannel(OrigChannel);
            udpCommandSocket.WriteOrigTDiv(OrigTiv);
        }

        #region 读取波形数据

        /// <summary>///控件读入能量图波形/// </summary>
        void OnEnergyArrayGraph(object sender, float[] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("能量波形数据已暂停");
                return;
            }
            int len = ConstUdpArg.ARRAY_USED;
            var graphdata = new float[len];

            Array.Copy(e, 0, graphdata, 0, len);

            for(int i = 0; i < len; i++) {
                if (Math.Abs(graphdata[i]) < 0.00001F) {
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
                xnums[i] = i + 1;
            }
            for(int i = 0; i < 64; i++) {
                dataPoints1[i] = new Point(xnums[i], graphdata[i]);
            }
            graph_energyFirst.Dispatcher.Invoke(() => {
                                                    graph_energyFirst.Refresh();
                                                    graph_energyFirst.DataSource = dataPoints1;
                                                });
        }

        /// <summary>///控件读入延迟波形/// </summary>
        void OnDelayWaveGraph(object sender, float[][] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("原始波形数据已暂停");
                return;
            }
            delay_graph.Dispatcher.Invoke(() => {
                                              delay_graph.Refresh();
                                              delay_graph.DataSource = e[DelayChannel];
                                          });
        }

        /// <summary>///控件读入原始波形/// </summary>
        void OnOrigGraph(object sender, float[][] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("工作时域波形数据已暂停");
                return;
            }
            orige_graph.Dispatcher.Invoke(() => {
                                              orige_graph.Refresh();
                                              orige_graph.DataSource = e[OrigTiv * ConstUdpArg.ORIG_CHANNEL_NUMS + OrigChannel];
                                          });
        }

        /// <summary>///控件读入工作时域波形/// </summary>
        void OnWorkGraph(object sender, float[] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("工作时域波形数据已暂停");
                return;
            }
            graph_normalTime.Dispatcher.Invoke(() => {
                                                   graph_normalTime.Refresh();
                                                   graph_normalTime.DataSource = e;
                                               });
        }

        /// <summary>
        ///     控件读入时域最大值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMaxWorkValue(object sender, float[] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
//                Console.WriteLine("工作时域波形数据已暂停");
                return;
            }
            float maxtmp = e.Max();
            tb_ampValue.Dispatcher.Invoke(() => {
                                              tb_ampValue.Text = maxtmp.ToString();
                                          });
        }

        /// <summary>///控件读入工作频率波形/// </summary>
        void OnFrapPointGraph(object sender, Point[] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("工作频率波形数据已暂停");
                return;
            }
            graph_normalFrequency.Dispatcher.Invoke(() => {
                                                        graph_normalFrequency.Refresh();
                                                        graph_normalFrequency.DataSource = e;
                                                    });
        }

        /// <summary>
        ///     读入频率求出最大值，发送给控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMaxFrapPoint(object sender, Point[] e) {
            if (e == null) {
                return;
            }
            if (IsGraphPause) {
                Console.WriteLine("工作频率波形数据已暂停");
                return;
            }
            Point maxPoint = new Point(0, 0);
            foreach(Point dPoint in e) {
                if (maxPoint.Y < dPoint.Y) {
                    maxPoint = dPoint;
                }
            }
            tb_frqValue.Dispatcher.Invoke(() => {
                                              tb_frqValue.Text = maxPoint.X.ToString();
                                          });
        }

        #endregion

        #region 点击响应函数

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
                selectdInfo.IsSaveData = true;
            }

            else {
                btnSave.Content = "保存数据";
                dataFile.DisableSaveFile();
            }
        }

        /// <summary>
        ///     连接正常工作数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WorkDataStart_OnClick(object sender, RoutedEventArgs e) {
            try {
                udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                if (NormWaveData == null) {
                    NormWaveData = new UdpWaveData();
                    NormWaveData.StartReceiveData(ConstUdpArg.Src_NormWaveIp);
                    btn_normalstart.Content = "停止";
                    NormWaveData.StartRcvEvent.Set();
                }
                else if (NormWaveData != null || NormWaveData.IsBuilded) {
                    NormWaveData.Dispose();
                    NormWaveData = null;
                    IsGraphPause = false;
                    graph_normalTime.Dispatcher.Invoke(() => {
                                                           graph_normalTime.DataSource = 0;
                                                       });
                    graph_normalTime.Refresh();
                    graph_normalFrequency.Dispatcher.Invoke(() => {
                                                                graph_normalFrequency.DataSource = 0;
                                                            });
                    graph_normalFrequency.Refresh();
                    graph_energyFirst.Dispatcher.Invoke(() => {
                                                            graph_energyFirst.DataSource = 0;
                                                        });
                    graph_energyFirst.Refresh();
                    btn_normalstart.Content = "启动";
                }
            }
            catch {
                Console.WriteLine(@"正常工作波形采集失败...");
                MessageBox.Show(@"正常工作波形采集失败...");
            }
        }

        /// <summary>
        ///     原始数据图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OrigDataStart_OnClick(object sender, RoutedEventArgs e) {
            MainStopwatch.Start();
            try {
                udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
                if (OrigWaveData == null) {
                    OrigWaveData = new UdpWaveData();
                    OrigWaveData.StartReceiveData(ConstUdpArg.Src_OrigWaveIp);
                    udpCommandSocket.WriteOrigChannel(OrigChannel);
                    udpCommandSocket.WriteOrigTDiv(OrigTiv);
                    btn_origstart.Content = "停止";
                    OrigWaveData.StartRcvEvent.Set();
                }
                else if (OrigWaveData != null || OrigWaveData.IsBuilded) {
                    OrigWaveData.Dispose();
                    OrigWaveData = null;
                    btn_origstart.Content = "启动";
                    IsGraphPause = false;
                    orige_graph.Dispatcher.Invoke(() => {
                                                      orige_graph.DataSource = 0;
                                                  });
                    orige_graph.Refresh();
                }
            }
            catch {
                Console.WriteLine(@"网络地址错误...");
            }
        }

        /// <summary>
        ///     开启DelayWave工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DelayDataStart_OnClick(object sender, RoutedEventArgs e) {
            MainStopwatch.Start();
            Task.Run(() => {
                         try {
                             udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToDeleyWindow);
                             if (DelayWaveData == null) {
                                 DelayWaveData = new UdpWaveData();
                                 DelayWaveData.StartReceiveData(ConstUdpArg.Src_DelayWaveIp);
                                 btn_delaystart.Dispatcher.InvokeAsync(() => {
                                                                           btn_delaystart.Content = "停止";
                                                                       });
                                 MainStopwatch.Stop();
                                 Console.WriteLine(MainStopwatch.ElapsedMilliseconds);
                                 MainStopwatch.Start();
                                 DelayWaveData.StartRcvEvent.Set();
                             }
                             else if (DelayWaveData != null || DelayWaveData.IsBuilded) {
                                 DelayWaveData.Dispose();
                                 IsGraphPause = false;
                                 DelayWaveData = null;
                                 btn_delaystart.Dispatcher.InvokeAsync(() => {
                                                                           btn_delaystart.Content = "启动";
                                                                       });
                                 delay_graph.Dispatcher.Invoke(() => {
                                                                   delay_graph.Refresh();
                                                               });
                             }
                             else if (DelayWaveData.IsBuilded && !DelayWaveData.IsRcving) {
                                 DelayWaveData.StartRcvEvent.Set();
                                 btn_delaystart.Dispatcher.InvokeAsync(() => {
                                                                           btn_delaystart.Content = "停止";
                                                                       });
                             }
                         }
                         catch(Exception exception) {
                             Console.WriteLine(@"错误代码{0}", exception);
                             Console.WriteLine(@"网络地址错误...");
                             MessageBox.Show(@"网络地址错误...");
                         }
                     });
        }

        void Btn_Path_OnClick(object sender, RoutedEventArgs e) {
            FolderBrowserDialog folderdDialog = new FolderBrowserDialog();
            string path = Environment.CurrentDirectory;
            folderdDialog.ShowNewFolderButton = true;
            if (folderdDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) { }
        }

        /// <summary>
        ///     读取文本数据值
        /// </summary>
        /// <param name="filepath">文本路径</param>
        /// <param name="valuedex"></param>
        /// <returns></returns>
        float[] ReadDiscValue(string filepath, int valuedex) {
            var rerList = new List<float>();
            using(FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read)) {
                using(StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("gb2312"))) {
                    string line;
                    string pattern = @"[+-]?\d+[\.]?\d*"; //包括带小数点的数字和整数
                    float decvalue;
                    while((line = sr.ReadLine()) != null) {
                        Console.WriteLine(line);
                        MatchCollection matchCollection = Regex.Matches(line, pattern);
                        Match newMatch = matchCollection[valuedex];
                        Console.WriteLine(newMatch.Value);
                        float.TryParse(newMatch.Value, out decvalue);
                        rerList.Add(decvalue);
                    }
                }
            }
            return rerList.ToArray();
        }

        /// <summary>
        ///     发送初始相位和B值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendConfig_OnClick(object sender, RoutedEventArgs e) {
            try {
                SendBvalue();
                SendPhase();
                MessageBox.Show("发送成功");
            }
            catch(Exception exception) {
                Console.WriteLine(exception);
                throw;
            }
            
            
        }

        /// <summary>
        ///     发送B值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendBvalue() {
            if (!udpCommandSocket.IsSocketConnect) {
                return;
            }
            string dirpath;
            string inipath = Environment.CurrentDirectory + "\\data";
            dirpath = inipath;

            string filename = "B值.txt";
            string filepath = dirpath + "\\" + filename;
            var bfloats = ReadDiscValue(filepath, 2);
            WaveSocket socket = new WaveSocket();
            var result = socket.GetSendBvalues(bfloats);

            udpCommandSocket.WriteBvalue(result);
        }

        /// <summary>
        ///     发送初始相位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendPhase() {
            if (!udpCommandSocket.IsSocketConnect) {
                return;
            }
            string dirpath;
            string inipath = Environment.CurrentDirectory + "\\data";
            dirpath = inipath;

            string filename = "初始相位.txt";
            string filepath = dirpath + "\\" + filename;
            var bfloats = ReadDiscValue(filepath, 1);
            WaveSocket socket = new WaveSocket();
            var result = socket.GetSendPhases(bfloats);
            udpCommandSocket.WritePhase(result);
        }

        /// <summary>
        ///     保存参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SaveConfig_OnClick(object sender, RoutedEventArgs e) {
            if (!udpCommandSocket.IsSocketConnect) {
                return;
            }
            NormalBValueSave(Bvalues);
            PhaseSave(Phases);
        }

        void TestBvalueSave() {
            Bvalues = new float[ConstUdpArg.ORIG_TIME_NUMS * ConstUdpArg.ORIG_CHANNEL_NUMS];
            var blist = new List<List<float>>();
            for(int i = 0; i < 8; i++) {
                blist.Add(new List<float>());
            }
            for(int i = 0; i < Bvalues.Length; i++) {
                blist[i % 8].Add(Bvalues[i]);
            }
            var package = new List<float>();
            for(int i = 0; i < 24; i++) {
                package.Add(0.000000f);
            }
            for(int i = 0; i < blist.Count; i++) {
                blist[i].AddRange(package);
            }
            var zList = new List<float>();
            for(int i = 0; i < blist.Count; i++) {
                zList.AddRange(blist[i].ToArray());
            }
            var senddata = zList.ToArray();
            string dirpath;
            string inipath = Environment.CurrentDirectory + "\\data";
            dirpath = inipath;

            string filename = "B值.txt";
            string filepath = dirpath + "\\" + filename;
            using(FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write)) {
                using(StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"))) {
                    if (Bvalues == null) {
                        return;
                    }
                    for(int i = 0; i < senddata.Length; i++) {
                        int tiv = i / 8;
                        int chl = i % 8;
                        string tmp = string.Format("通道{0}时分{1}B={2}", chl + 1, tiv + 1, senddata[i]);

                        sw.WriteLine(tmp);
                    }
                }
            }
        }

        void PhaseSave(float[] bPhases) {
            if (bPhases == null) {
                return;
            }
            string dirpath;
            string inipath = Environment.CurrentDirectory + "\\data";
            dirpath = inipath;
            string filename = "初始相位.txt";
            string filepath = dirpath + "\\" + filename;
            using(FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write)) {
                using(StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"))) {
                    for(int i = 0; i < bPhases.Length; i++) {
                        string tmp = string.Format("通道{0} Phase={1}", i + 1, bPhases[i]);
                        sw.WriteLine(tmp);
                    }
                }
            }
        }

        void NormalBValueSave(float[] bFloats) {
            if (bFloats == null) {
                return;
            }
            string dirpath;
            string inipath = Environment.CurrentDirectory + "\\data";
            dirpath = inipath;
            string filename = "B值.txt";
            string filepath = dirpath + "\\" + filename;
            using(FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write)) {
                using(StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("gb2312"))) {
                    for(int i = 0; i < bFloats.Length; i++) {
                        int tiv = i / 8;
                        int chl = i % 8;
                        string tmp = string.Format("时分:{0}B通道{1}:{2}", tiv + 1, chl + 1, bFloats[i]);
                        sw.WriteLine(tmp);
                    }
                }
            }
        }

        /// <summary>
        ///     计算初始相位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CalPhase_OnClick(object sender, RoutedEventArgs e) {
            if (!udpCommandSocket.IsSocketConnect) {
                return;
            }
            btn_calPhase.IsEnabled = false;
            Phases = new float[ConstUdpArg.ORIG_TIME_NUMS];
            Task.Run(() => {
                         WaveSocket waveSocket = new WaveSocket();
                         waveSocket.StartCaclPhase(ConstUdpArg.Src_OrigWaveIp, udpCommandSocket);
                         waveSocket.SendOrigSwitchCommand(8, 1); //八个通道一个时分
                         waveSocket.RcvResetEvent.Set();
                         Phases = waveSocket.CalToPhase();
                         MessageBox.Show("初始相位计算成功");

                         btn_calPhase.Dispatcher.Invoke(() => {
                                                            btn_calPhase.IsEnabled = true;
                                                        });
                         waveSocket.Dispose();
                     });
        }

        /// <summary>
        ///     计算B值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CalBvalue_OnClick(object sender, RoutedEventArgs e) {
            if (!udpCommandSocket.IsSocketConnect) {
                return;
            }
            btn_calBvalue.IsEnabled = false;
            Bvalues = new float[ConstUdpArg.ORIG_TIME_NUMS * ConstUdpArg.ORIG_CHANNEL_NUMS];
            Task.Run(() => {
                         WaveSocket waveSocket = new WaveSocket();
                         waveSocket.StartCaclBvalue(ConstUdpArg.Src_OrigWaveIp, udpCommandSocket);
                         waveSocket.SendOrigSwitchCommand(8, 8);
                         waveSocket.RcvResetEvent.Set();

                         var Bresults = new float[64];
                         for(int i = 0; i < waveSocket.BvalueDatas.Length; i++) {
                             var query = waveSocket.BvalueDatas[i];
                             Bresults[i] = (query.Max() - query.Min()) / (8192.0f * 2.0f); //short转换为float  （最大值-最小值）/2
                         }
                         Array.Copy(Bresults, Bvalues, Bresults.Length);
                         waveSocket.Dispose();
                         btn_calBvalue.Dispatcher.Invoke(() => {
                                                             btn_calBvalue.IsEnabled = true;
                                                         });
                         blistview.Dispatcher.Invoke(() => {
                                                         observableCollection.Clear();
                                                         foreach(float bvalue in Bresults) {
                                                             observableCollection.Add(new UIBValue(bvalue));
                                                         }
                                                         stopwatch.Stop();
                                                         MessageBox.Show("B值计算成功");
                                                         IsGetBvalue = true;
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

        void BtnPauseWorkGraph_OnClick(object sender, RoutedEventArgs e) {
            IsGraphPause = !IsGraphPause;
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
            udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
//            Dispose();
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
//                btnlisten.Content = "停止听音";
            }
            else {
                if (Dataproc.SoundEventHandler != null) {
                    Dataproc.SoundEventHandler -= OnSndData;
                }
                dxplaysnd.Close();
                dxplaysnd = null;
//                btnlisten.Content = "开始听音";
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
                        selectdInfo.WorkWaveChannel = 1;
                    }
                    selectdInfo.WorkWaveChannel = workchNum;
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
                udpCommandSocket.WriteOrigChannel(num - 1);
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

                udpCommandSocket.WriteOrigTDiv(num - 1);
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
                        channel = 1;
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

        void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectionChangedEventArgs slcArg = e;
            if (slcArg.RemovedItems.IsEmpty()) {
                return;
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HeaderedContentControl arg = slcArg.RemovedItems[0] as HeaderedContentControl;
            if (arg == null) {
                return;
            }

            IsGraphPause = false;
            switch(arg.Name) {
                case "delayItem": {
                    Task.Run(() => {
                                 udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToDeleyWindow);
                                 if (DelayWaveData != null) {
                                     btn_delaystart.Dispatcher.Invoke(() => {
                                                                          DelayDataStart_OnClick(null, null);
                                                                      });
                                 }
                                 Console.WriteLine("关闭延时波形");
                             });
                }
                    break;
                case "origItem": {
                    Task.Run(() => {
                                 udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
                                 if (OrigWaveData != null) {
                                     btn_origstart.Dispatcher.Invoke(() => {
                                                                         OrigDataStart_OnClick(null, null);
                                                                     });
                                 }
                                 Console.WriteLine("关闭原始波形");
                             });
                }
                    break;
                case "normalItem": {
                    Task.Run(() => {
                                 udpCommandSocket.SwitchWindow(ConstUdpArg.SwicthToNormalWindow);
                                 if (NormWaveData != null) {
                                     btn_normalstart.Dispatcher.Invoke(() => {
                                                                           WorkDataStart_OnClick(null, null);
                                                                       });
                                 }
                                 Console.WriteLine("关闭正常工作波形");
                             });
                }
                    break;
                case "configItem":
                    break;
                default: {
                    Console.WriteLine("切换窗口错误");
                }
                    break;
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

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

        public static OnSelectdInfo selectdInfo {
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

        public float[] Phases {
            get;
            set;
        }

        public bool IsGetBvalue {
            get;
            set;
        }

        public UdpWaveData NormWaveData {
            get;
            set;
        }

        public UdpWaveData OrigWaveData {
            get;
            set;
        }

        public UdpWaveData DelayWaveData {
            get;
            set;
        }

        public bool IsGraphPause {
            get;
            set;
        }

        public Stopwatch MainStopwatch {
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
        DiscFile.DataFile dataFile;
        public static int sndCoefficent = 50;
        static DxPlaySound dxplaysnd; //播放声音对象
        UdpCommandSocket udpCommandSocket;
        Dataproc dataproc;
        //public ConstUdpArg ConstUdpArg;
        bool isworkSaveFlag;
        bool isorigSaveFlag;
        int origChannel;

        DispatcherTimer timer_bvalue;
        DispatcherTimer timer_blist;

        ObservableCollection<UIBValue> observableCollection;
        Stopwatch stopwatch;

        #endregion

        #region 系统信息,(读/写/存)按钮响应

        #region 脉冲周期.(读/写/存).按钮响应

        /// <summary>脉冲周期.读.按钮响应</summary>
        void OnPulsePeriodRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadPulsePeriod();
            Task.Run(() => {
                         btn_readpluseperiod.Dispatcher.Invoke(() => {
                                                                   btn_readpluseperiod.IsEnabled = false;
                                                               });

                         Thread.Sleep(1000);
                         btn_readpluseperiod.Dispatcher.Invoke(() => {
                                                                   btn_readpluseperiod.IsEnabled = true;
                                                               });
                     });
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
            udpCommandSocket.WritePulsePeriod(data);
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
            udpCommandSocket.SavePulsePeriod(data);
        }

        #endregion

        #region 脉冲延时.(读/写/存).按钮响应

        /// <summary>脉冲延时.读.按钮响应</summary>
        void OnPulseDelayRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadPulseDelay();
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
            udpCommandSocket.WritePulseDelay(data);
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
            udpCommandSocket.SavePulseDelay(data);
        }

        #endregion

        #region 脉冲宽度.(读/写/存).按钮响应

        /// <summary>脉冲宽度.读.按钮响应</summary>
        void OnPulseWidthRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadPulseWidth();
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
            udpCommandSocket.WritePulseWidth(data);
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
            udpCommandSocket.SavePulseWidth(data);
        }

        #endregion

        #region ADC偏移.(读/写/存).按钮响应

        /// <summary>ADC偏移.读.按钮响应</summary>
        void OnAdcOffsetRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadAdcOffset();
        }

        /// <summary>ADC偏移.写.按钮响应</summary>
        void OnAdcOffsetWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_adc_offset.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            float floatT = float.Parse(strT);
            if (floatT > 0.5) {
                floatT = 0.5f;
            }
            int tmpT = (int) ((floatT + 1.2) * 128 / 1.2);

            udpCommandSocket.WriteAdcOffset((byte) tmpT);
        }

        /// <summary>ADC偏移.存.按钮响应</summary>
        void OnAdcOffsetSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_setting_adc_offset.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }

            float floatT = float.Parse(strT);
            int tmpT = (int) ((floatT + 1.2) * 128 / 1.2);
            udpCommandSocket.SaveAdcOffset((byte) tmpT);
        }

        #endregion

        #region 延时信息.(读/写/存).按钮响应

        /// <summary>///延时信息.读.按钮响应 /// </summary>
        void OnDelayTimeRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadDelyTime();
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
            udpCommandSocket.WriteDelayTime(data);
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

            udpCommandSocket.SaveDelayTime(data);
        }

        #endregion

        #region Dac信息.(读/写/存).按钮响应

        void DacLenRead_OnClick(object sender, RoutedEventArgs e) {
            udpCommandSocket.ReadCanChannelLen();
        }

        void DacLenWrite_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_dacLen.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            int t = int.Parse(strT);

            udpCommandSocket.WriteDacChannel(t);
        }

        void DacLenSave_OnClick(object sender, RoutedEventArgs e) {
            string strT = tb_dacLen.Text;
            if (string.IsNullOrEmpty(strT)) {
                return;
            }
            int intT = int.Parse(strT);
            int a = intT / 256;
            int b = intT - a * 256;

            var data = new byte[2];
            data.SetValue((byte) a, 0);
            data.SetValue((byte) b, 1);

            udpCommandSocket.SaveDacLen(data);
        }

        #endregion

        #endregion
    }
}
