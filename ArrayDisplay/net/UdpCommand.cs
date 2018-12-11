﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
﻿using System.Windows.Media.TextFormatting;
﻿using ArrayDisplay.UI;

namespace ArrayDisplay.net {
    public class UdpCommand : IDisposable {
        public UdpCommand() {
            rcvsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            rcvsocket.ReceiveTimeout = 5000;
            sedsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sedsocket.SendTimeout = 5000;
        }

        /// <summary>
        ///     初始化Socket，绑定IP
        /// </summary>
        public void Init() {
            try {
                rcvsocket.Bind(RcvIp);
                sedsocket.Bind(SedIp);

                isSocketInit = true;
            }
            catch(Exception e) {
                Console.WriteLine(@"创建UDP失败...错误为{0}", e);
                MessageBox.Show(@"创建UDP失败...");
            }
        }

        /// <summary>///切换功能窗口（波形或命令）/// </summary>
        public void SwitchWindow(byte[] cmdBytes) {
            if (!isSocketInit) {
                SedIp = ConstUdpArg.Src_ComMsgIp;
                RcvIp = ConstUdpArg.Src_ComDatIp;
                Init();
            }
            var tempBytes = new byte[8];
            cmdBytes.CopyTo(tempBytes, 0);
            Console.WriteLine("切换窗口:{0}", BitConverter.ToString(tempBytes));
            Send(tempBytes, ConstUdpArg.Dst_ComMsgIp);
        }

        #region Property

        public IPEndPoint SedIp {
            get;
            set;
        }

        public IPEndPoint RcvIp {
            get;
            set;
        }

        #endregion

        #region Field

        readonly Socket rcvsocket;
        readonly Socket sedsocket;
        public bool isSocketInit;

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (rcvsocket != null) {
                    rcvsocket.Dispose();
                }
                if (sedsocket != null) {
                    sedsocket.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 获取系统状态

        /// <summary>
        ///     获取系统状态
        ///     一次发送9条指令,包含:设备类型,设备ID,设备MAC,ADC_Err,脉冲周期,脉冲延时,脉冲宽度,ADC偏移,ADC禁用
        ///     接收反馈信息长度分别为:26bit,26bit,24bit,21bit,20bit,20bit,20bit,19bit,19bit
        /// </summary>
        public void GetDeviceState() {
            if (!isSocketInit) {
                SedIp = ConstUdpArg.Src_ComMsgIp;
                RcvIp = ConstUdpArg.Src_ComDatIp;
                Init();
            }
            //读取:设备类型
            ReadDeviceType();
            //读取:设备ID
            ReadDeviceId();
            //读取:设备MAC
            ReadDeviceMac();
            //读取:脉冲周期
            ReadPulsePeriod();
            //读取:脉冲延时
            ReadPulseDelay();
            //读取:脉冲宽度
            ReadPulseWidth();
            //读取:ADC偏移
            ReadAdcOffset();
        }

        #region 读(发送指令及数据)

        /// <summary>读取:设备类型</summary>
        public void ReadDeviceType() {
            Send(ConstUdpArg.Device_Type, ConstUdpArg.Dst_ComMsgIp);
            ReceiveDeviceType(26);
        }

        /// <summary>读取:设备ID</summary>
        public void ReadDeviceId() {
            Send(ConstUdpArg.Device_Id, ConstUdpArg.Dst_ComMsgIp);
            ReceiveDeviceId(26);
        }

        /// <summary>读取:设备MAC</summary>
        public void ReadDeviceMac() {
            Send(ConstUdpArg.Device_Mac, ConstUdpArg.Dst_ComMsgIp);
            ReceiveDeviceMac(24);
        }

        /// <summary>读取:脉冲周期</summary>
        public void ReadPulsePeriod() {
            Send(ConstUdpArg.Pulse_Period_Read, ConstUdpArg.Dst_ComMsgIp);
            ReceivePulsePeriod(20);
        }

        /// <summary>读取:脉冲延时</summary>
        public void ReadPulseDelay() {
            Send(ConstUdpArg.Pulse_Delay_Read, ConstUdpArg.Dst_ComMsgIp);
            ReceivePulseDelay(20);
        }

        /// <summary>读取:脉冲宽度</summary>
        public void ReadPulseWidth() {
            Send(ConstUdpArg.Pulse_Width_Read, ConstUdpArg.Dst_ComMsgIp);
            ReceivePulseWidth(20);
        }

        /// <summary>读取:ADC偏移</summary>
        public void ReadAdcOffset() {
            Send(ConstUdpArg.GetAdcOffsetRead(DisPlayWindow.systemInfo.AdcNum), ConstUdpArg.Dst_ComMsgIp);
            ReceiveAdcOffset(19);
        }

        /// <summary>///读取延时信息/// </summary>
        public void ReadDelyTime() {
            var sedip = ConstUdpArg.GetDelayTimeReadCommand(DisPlayWindow.systemInfo.ChannelDelayNums);
            Send(sedip, ConstUdpArg.Dst_ComMsgIp);
            ReceiveChannelDeleyTime(20);
        }

        #endregion

        #region 写(发送指令及数据)

        /// <summary>写:脉冲周期</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulsePeriod(byte[] data) {
            WriteData(ConstUdpArg.Pulse_Period_Write, data);
        }

        /// <summary>写:脉冲延时</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulseDelay(byte[] data) {
            WriteData(ConstUdpArg.Pulse_Delay_Write, data);
        }

        /// <summary>写:脉冲宽度</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulseWidth(byte[] data) {
            WriteData(ConstUdpArg.Pulse_Width_Write, data);
        }

        /// <summary>写:ADC偏移</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void WriteAdcOffset(byte data) {
            int adcNum = int.Parse(DisPlayWindow.hMainWindow.setting_adc_num.Text);
            var addr = ConstUdpArg.GetAdcOffsetWrite(adcNum);
            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            //最后一位是需要发送的数据
            sendData.SetValue(data, sendData.Length - 1);

            Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>写:脉冲宽度</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WriteDelayTime(byte[] data) {
            var tmp = ConstUdpArg.GetDelayTimeWriteCommand(int.Parse(DisPlayWindow.hMainWindow.tb_deleyChannel.Text) - 1);
            WriteData(tmp, data);
        }
        /// <summary>
        /// 发送改变通道号
        /// </summary>
        /// <param name="num"></param>
        public void WriteOrigChannel(int num) {
            var data = ConstUdpArg.OrigChannel_Write;
            byte[] temp = new byte[1];
             temp[0] = (byte)num;
            WriteData(data,temp);
        }
        /// <summary>
        /// 发送改变时分号
        /// </summary>
        /// <param name="num"></param>
        public void WriteOrigTDiv(int num) {
            var data = ConstUdpArg.OrigTimDiv_Write;
            byte[] temp = new byte[1];
            temp[0] = (byte)num;
            WriteData(data, temp); 
        }

        #endregion

        #region 存(发送指令及数据)

        /// <summary>存:脉冲周期</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulsePeriod(byte[] data) {
            SaveData(ConstUdpArg.Pulse_Period_Save, data);
        }

        /// <summary>存:脉冲延时</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulseDelay(byte[] data) {
            SaveData(ConstUdpArg.Pulse_Delay_Save, data);
        }

        /// <summary>存:脉冲宽度</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulseWidth(byte[] data) {
            SaveData(ConstUdpArg.Pulse_Width_Save, data);
        }

        /// <summary>存:ADC偏移</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void SaveAdcOffset(byte data) {
            int adcNum = int.Parse(DisPlayWindow.hMainWindow.setting_adc_num.Text);
            var addr = ConstUdpArg.GetAdcOffsetSave(adcNum);
            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            //最后一位是需要发送的数据
            sendData.SetValue(data, sendData.Length - 1);

            Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>存:延时通道</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void SaveDelayTime(byte[] data) {
            var cmd = ConstUdpArg.GetDelayTimeSaveCommand(int.Parse(DisPlayWindow.hMainWindow.tb_deleyChannel.Text));
            SaveData(cmd, data);
        }

        /// <summary>存:ADC禁用</summary>
        /// <summary>
        ///     执行写指令
        /// </summary>
        /// <param name="addr">指令及地址</param>
        /// <param name="data">待发送的数据</param>
        void WriteData(byte[] addr, byte[] data) {
            var sendData = new byte[addr.Length + data.Length];
            Array.Copy(addr,0, sendData,0, addr.Length);

            //最后两位是需要发送的数据
            Array.Copy(data, 0, sendData, sendData.Length - data.Length, data.Length);

            Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>
        ///     执行存指令
        /// </summary>
        /// <param name="addr">指令及地址</param>
        /// <param name="data">待发送的数据</param>
        void SaveData(byte[] addr, byte[] data) {
            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            for(int i = 0; i < data.Length; i++) {
                byte d = data[i];
                //每循环一次地址(倒数第二位)+1
                sendData.SetValue((byte) (sendData[sendData.Length - 2] + i), sendData.Length - 2);
                //最后一个元素是数据
                sendData.SetValue(d, sendData.Length - 1);
                //发送
                Send(sendData, ConstUdpArg.Dst_ComMsgIp);
                Thread.Sleep(1000);
            }
        }

        #endregion

        #endregion

        #region 数据发送

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="bytes">待发送byte[]数据</param>
        /// <param name="endPoint">下位机IP与端口号</param>
        /// <returns></returns>
        public void Send(byte[] bytes, IPEndPoint endPoint) {
            Task.Factory.StartNew(() => {
                                      StreamDataPack dataPack = new StreamDataPack {Ip = endPoint, DataBytes = bytes};
                                      var sendbytes = dataPack.DataBytes;
                                      dataPack.DataBytes.CopyTo(sendbytes, 0);
                                      IPEndPoint sendip = dataPack.Ip;
                                      EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);

                                      try {
                                          sedsocket.SendTo(sendbytes, sendip);
                                      }
                                      catch(Exception e) {
                                          Console.WriteLine(e);
                                          throw;
                                      }
                                      Console.WriteLine("发送数据:{0}", BitConverter.ToString(sendbytes, 0, sendbytes.Length));

                                      var rcvUdpBuffer = new byte[18];
                                      try {
                                          int offset = 0;
                                          while(offset < rcvUdpBuffer.Length)
                                              try {
                                                  int ret = sedsocket.ReceiveFrom(rcvUdpBuffer, offset, rcvUdpBuffer.Length - offset, SocketFlags.None,
                                                                                  ref senderRemote);
                                                  offset += ret;
                                              }
                                              catch(Exception e) {
                                                  Console.WriteLine(e);
                                                  throw;
                                              }
                                          var flagBytes = new byte[8];
                                          Array.Copy(rcvUdpBuffer, 0, flagBytes, 0, 8);
                                          string temp = BitConverter.ToString(flagBytes);
//                Console.WriteLine("接收数据:{0}", temp);
                                      }
                                      catch(Exception e) {
                                          Console.WriteLine(e);
                                          throw;
                                      }
                                  });
        }

        /// <summary>
        ///     线程处理函数：发送指令
        /// </summary>
        /// <param name="databytes">待发送的UdpData数据，包括IP和数据</param>
        void ThreadSend(object databytes) { }

        #endregion

        #region 系统参数(读),数据接收

        #region 接收设备类型数据

        /// <summary>
        ///     接收设备类型数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceType(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);

                                      string mcType = Encoding.ASCII.GetString(rcvUdpBuffer, 6, 8);
                                      Console.WriteLine("mc_type={0}", mcType);
                                      DisPlayWindow.systemInfo.McType = mcType;
                                  });
        }

        #endregion

        #region 接收设备ID数据

        /// <summary>
        ///     接收设备ID数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceId(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      string mcId = "";
                                      for(int i = 0; i < 8; i++) {
                                          mcId += BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                                      }
                                      Console.WriteLine("mc_id={0}", mcId);
                                      DisPlayWindow.systemInfo.McId = mcId;
                                  });
        }

        #endregion

        #region 接收设备Mac数据

        /// <summary>
        ///     接收设备ID数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceMac(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      string mcMac = "";
                                      for(int i = 0; i < 6; i++) {
                                          if (i > 0) {
                                              mcMac += ":" + BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                                          }
                                          else {
                                              mcMac += BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                                          }
                                      }
                                      Console.WriteLine("mc_mac={0}", mcMac);
                                      DisPlayWindow.systemInfo.McMac = mcMac;
                                  });
        }

        #endregion

        #region 接收脉冲周期数据

        public void ReceivePulsePeriod(int bufferLength) {
            var rcvUdpBuffer = ReadRemote(bufferLength);

            Task.Factory.StartNew(() => {
                                      //返回数据以指令为开头
                                      var cmd = new byte[6];
                                      Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
                                      var temp = new byte[2];
                                      Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
                                      SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
                                  });
        }

        #endregion

        #region 接收脉冲延时数据

        public void ReceivePulseDelay(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      //返回数据以指令为开头
                                      var cmd = new byte[6];
                                      Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
                                      var temp = new byte[2];
                                      Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
                                      SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
                                  });
        }

        #endregion

        #region 接收脉冲宽度数据

        public void ReceivePulseWidth(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      //返回数据以指令为开头
                                      var cmd = new byte[6];
                                      Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
                                      var temp = new byte[2];
                                      Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
                                      SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
                                  });
        }

        #endregion

        #region 接收ADC偏移数据

        public void ReceiveAdcOffset(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      //返回数据以指令为开头
                                      var cmd = new byte[6];
                                      Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
                                      var temp = new byte[1];
                                      Array.Copy(rcvUdpBuffer, 6, temp, 0, 1);
                                      SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
                                  });
        }

        #endregion

        #region 接收通道延时数据

        public void ReceiveChannelDeleyTime(int bufferLength) {
            Task.Factory.StartNew(() => {
                                      var rcvUdpBuffer = ReadRemote(bufferLength);
                                      //返回数据以指令为开头
                                      var cmd = new byte[6];
                                      Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
                                      var temp = new byte[2];
                                      Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
                                      SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
                                  });
        }

        #endregion

        /// 将接收到的(脉冲周期/脉冲延时/脉冲宽度/ADC偏移)数据转换为界面显示的值
        /// <param name="data"></param>
        /// <returns></returns>
        int ToInt(byte[] data) {
            if (data == null || data.Length < 1) {
                return -1;
            }
            if (data.Length != 2) {
                throw new Exception("异常(脉冲周期/脉冲延时/脉冲宽度)数据.");
            }

            int hig = data[0] * 256;
            int low = data[1];
            return hig + low;
        }

        /// 读取远程发送的UDP数据
        /// <param name="bufferLength">缓存区大小</param>
        /// <returns></returns>
        byte[] ReadRemote(int bufferLength) {
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            var rcvUdpBuffer = new byte[bufferLength * 4];

            int offset = 0;
            try {
                rcvsocket.ReceiveFrom(rcvUdpBuffer, offset, rcvUdpBuffer.Length - offset, SocketFlags.None, ref senderRemote);
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }

            return rcvUdpBuffer;
        }

        /// 将接收到的数据显示到对应的界面上
        /// 由于UDP返回数据包可能不是按发送命令顺序,此处增加识别处理
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        void SetData(string cmd, byte[] data) {
            //得到发送指令后与ConstUdpArg中指令集对比,再将对应的数据处理后显示到界面上
            if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Period_Read, 0, 6).Equals(cmd)) //脉冲周期
            {
                int pulsePeriod = ToInt(data) * 5;
                Console.WriteLine("pulse_period={0}", pulsePeriod);
                DisPlayWindow.systemInfo.PulsePeriod = pulsePeriod;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Delay_Read, 0, 6).Equals(cmd)) //脉冲延时
            {
                int pulseDelay = ToInt(data) * 5;
                Console.WriteLine("pulse_delay={0}", pulseDelay);
                DisPlayWindow.systemInfo.PulseDelay = pulseDelay;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Width_Read, 0, 6).Equals(cmd)) //脉冲宽度
            {
                int pulseWidth = ToInt(data) * 5;
                Console.WriteLine("pulse_width={0}", pulseWidth);
                DisPlayWindow.systemInfo.PulseWidth = pulseWidth;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.GetAdcOffsetRead(DisPlayWindow.systemInfo.AdcNum), 0, 6).Equals(cmd)) //ADC偏移
            {
                double adcOffset = data[0];
                adcOffset = adcOffset * 1.2 / 128 - 1.2;
                //AdcOffset.ToString("G3");
                Console.WriteLine("adc_offset={0}", adcOffset.ToString("G2"));
                DisPlayWindow.systemInfo.AdcOffset = adcOffset.ToString("G2");
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.GetDelayTimeReadCommand(DisPlayWindow.systemInfo.ChannelDelayNums), 0, 6).Equals(cmd)) //延迟通道
            {
                byte t = data[0];
                data[0] = data[1];
                data[1] = t;
                int delaytime = BitConverter.ToUInt16(data, 0);
                DisPlayWindow.systemInfo.ChannelDelayTime = delaytime;
            }
            else {
//其他,未定义
                Console.WriteLine("cmd={0}", cmd);
            }
        }

        #endregion

        
    }
}
