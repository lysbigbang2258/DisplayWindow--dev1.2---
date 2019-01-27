// 2019010710:15 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ArrayDisplay.UI;

namespace ArrayDisplay.net {
    public class WaveSocket:IDisposable  {

        public Socket MSocket {
            get;
            set;
        }

        public Thread RcvThread {
            get;
            set;
        }

        public short[][] BvalueData {
            get;
            set;
        }


        public short[][] RcvPhaseData {
            get;
            set;
        }

        public float[] PhaseFloats {
            get;
            set;
        }

        public AutoResetEvent RcvResetEvent {
            get;
            set;
        }

        public UdpCommand UCommand {
            get;
            set;
        }

        public WaveSocket() {
            MSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                    ProtocolType.Udp);
        }

        public void StartCaclBvalue(IPEndPoint ip,UdpCommand udpCommand) {
            try {
                UCommand = udpCommand;
                MSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH;      
                MSocket.Bind(ip);
                RcvThread = new Thread(RcvUdpdataToBvalue) {IsBackground = true};
                RcvResetEvent = new AutoResetEvent(false);
                BvalueData = new short[64][];
                RcvThread.Start();
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        public void StartCaclPhase(IPEndPoint ip, UdpCommand udpCommand)
        {
            try {
                UCommand = udpCommand;
                MSocket.Blocking = true;
                MSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH;
                MSocket.Bind(ip);
                RcvThread = new Thread(RcvUdpdataToPhase) { IsBackground = true };
                RcvResetEvent = new AutoResetEvent(false);
                RcvPhaseData = new short[8][]; 
                PhaseFloats = new float[8];
                RcvThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        /// <summary>
        /// 接收网络流相位信号
        /// </summary>
        void RcvUdpdataToPhase()
        {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>> {
                new ArraySegment<byte>(new byte[1282])
            };
            int ret;
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                RcvResetEvent.WaitOne();
                try
                {
                    stopwatch.Start();
                    ret = MSocket.Receive(buffer);
                    if (ret <= 0)
                    {
                        continue;
                    }
                    int offset = 0;
                    var bytedata = buffer[0].Array;
                    if (bytedata == null)
                    {
                        continue;
                    }
                    int chl = bytedata[0];
                    int tiv = bytedata[1];

                    int index = tiv * 8 + chl;
                    offset += 2;
                    short[] sbdata = new short[(bytedata.Length - offset) / 2];
                    var purdata = new byte[bytedata.Length - offset];
                    Array.Copy(bytedata, offset, purdata, 0, purdata.Length);
                    for (int i = 0; i < sbdata.Length; i++)//大端数据
                    {
                        var t = new byte[2];
                        t[0] = purdata[2 * i + 1];
                        t[1] = purdata[2 * i];
                        sbdata[i] = BitConverter.ToInt16(t, 0);
                    }
                    if (index<0 || index>31) {
                        Console.WriteLine("Why");
                    }
                    RcvPhaseData[index] = new short[sbdata.Length];
                    Console.WriteLine("通道： " + index);
                    Array.Copy(sbdata, RcvPhaseData[index], sbdata.Length);    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                stopwatch.Stop();
                Console.WriteLine("RCVTime:" + stopwatch.ElapsedMilliseconds);
                RcvResetEvent.Reset();
            }

        }
        /// <summary>
        /// 采集信号保存到Bvalue[]数组
        /// </summary>
        void RcvUdpdataToBvalue() {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>> {
                new ArraySegment<byte>(new byte[1282]),
            };
            int ret;
            Stopwatch stopwatch = new Stopwatch();
            while(true) {
                RcvResetEvent.WaitOne();
                try {
                    stopwatch.Start();
                    ret = MSocket.Receive(buffer);
                    if (ret <= 0)
                    {
                        continue;
                    }
                    int offset = 0;
                    var bytedata = buffer[0].Array;
                    if (bytedata == null) {
                        continue;
                    }
                    int tiv = bytedata[0];
                    int chl = bytedata[1];                    
                    int index = chl * 8 + tiv;
                    offset += 2;
                    short[] sbdata = new short[(bytedata.Length - offset) / 2];
                    var purdata = new byte[bytedata.Length - offset];
                    
                    Array.Copy(bytedata, offset, purdata, 0, purdata.Length);
                    for (int i = 0; i < sbdata.Length; i++)//大端数据
                    {
                        var t = new byte[2];
                        t[0] = purdata[2 * i + 1];
                        t[1] = purdata[2 * i];
                        sbdata[i] = BitConverter.ToInt16(t, 0);
                    }
                    BvalueData[index] = new short[sbdata.Length];
                    double progressvalue = (index + 1) * 100.0 / 64;
                    Console.WriteLine("通道： " + index);
                    Array.Copy(sbdata, BvalueData[index], sbdata.Length);
                    DisPlayWindow.hMainWindow.bvaulue_pgbar.Dispatcher.Invoke(() =>
                                                                              {
                                                                                  DisPlayWindow.hMainWindow.bvaulue_pgbar.Value = progressvalue;
                                                                              });//进度条
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                stopwatch.Stop();
                Console.WriteLine("RCVTime:"+stopwatch.ElapsedMilliseconds);
               
                RcvResetEvent.Reset();
            }
            
        }
        /// <summary>
        /// 循环发送命令
        /// </summary>
        /// <param name="chinum">通道个数</param>
        /// <param name="timenum">时分个数</param>
        public void SendOrigSwitchCommand(int chinum,int timenum)
        {
            UCommand.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
//            UCommand.WriteOrigTDiv(0);
//            Console.WriteLine("发送时分0");
//            UCommand.WriteOrigChannel(0);
//            Console.WriteLine("发送通道0");

            for (int i = 0; i < timenum; i++)
            {
                for (int j = 0; j < chinum; j++)
                {
                    UCommand.WriteOrigTDiv(i);
                    Console.WriteLine("发送时分{0}", i);
                    UCommand.WriteOrigChannel(j);
                    Console.WriteLine("发送通道{0}", j);
                    Thread.Sleep(15);
                    RcvResetEvent.Set();
                    Thread.Sleep(20);
                }
            }
            Thread.Sleep(100);
            UCommand.SwitchWindow(ConstUdpArg.SwicthToOriginalWindow);
            UCommand.WriteOrigChannel(0);
            UCommand.WriteOrigTDiv(0);
            UCommand.SwitchWindow(ConstUdpArg.SwicthToStateWindow);
        }

        public float[] CalToPhase() {
           return CalToPhase(RcvPhaseData);
        }

        /// <summary>
        /// 计算初始相位
        /// </summary>
        /// <param name="rcvPahseArray">采集原始数据值</param>
        /// <returns>初始相位float型</returns>
        float[] CalToPhase(short[][] rcvPahseArray)
        {
            var calphase = new short[40][];
            var calcos = new float[40][];
            var calsin = new float[40][];
            var meancos = new float[40];
            var meansin = new float[40];
            var caltin = new float[8];

            for (int i = 0; i < calphase.Length; i++)
            {
                calphase[i] = new short[16];
                calcos[i] = new float[16];
                calsin[i] = new float[16];

            }

            for (int index = 0; index < rcvPahseArray.Length; index++) {
                var array = rcvPahseArray[index];
                for (int i = 0; i < array.Length; i++)
                {
                    var frams = i / 16;
                    var num = i % 16;
                    calphase[frams][num] = array[i];
                }
                for (int i = 0; i < calphase.Length; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        calcos[i][j] =(float)(calphase[i][j] * Math.Cos(Math.PI / 8 * j));
                        calsin[i][j] =(float)(calphase[i][j] * Math.Sin(Math.PI / 8 * j)); 
                    }
                   
                }
                for(int i = 0; i < calphase.Length; i++) {
                    meansin[i] = calsin[i].Average();
                    meancos[i] = calcos[i].Average();
                }

                var msin = meansin.Average();
                var mcos = meancos.Average();
                var art = (float)Math.Atan(msin / mcos) * -1; 
                caltin[index] =art;
            }
            PhaseFloats = caltin;
            return caltin;

        }

       public byte[][] GetSendPhases(float[] caltin) {
            List<float> list1;
            List<float> list2;
            List<List<float>> oneLists = new List<List<float>>();
            List<float[]> doublelist = new List<float[]>();
            for(int index = 0; index < caltin.Length; index++) {
                list1 = new List<float>();
                for(int i = 0; i < 16; i++) {
                    var tmp1 = Math.Cos(Math.PI / 8 * i + caltin[index]);
                    var tmp2 = Math.Cos(Math.PI / 4 * i + 2*caltin[index]);
                    list1.Add((float)tmp1);
                    list1.Add((float)tmp2);
                }
                oneLists.Add(list1);
            }
            for(int i = 0; i < 8; i++) {
                var tmplist = new List<float>();
                    tmplist.AddRange(oneLists[i]);
                doublelist.Add(tmplist.ToArray());
            }
            
            var shortlist = doublelist.ConvertAll(FloatsToshorts);
            var bytelist = shortlist.ConvertAll(ShortTobytes);
            var sendbytes = bytelist.ToArray();
            return sendbytes;
        }
        
        static short[] FloatsToshorts(float[] input)
        {
            List<short> list = new List<short>();
            for (int i = 0; i < input.Length; i++)
            {
                short t = (short)(input[i] * Math.Pow(2, 12));
                list.Add(t);
            }
            return list.ToArray();
        }

        static byte[] ShortTobytes(short[] input)
        {
            List<byte> list = new List<byte>();
            var tmp =new byte[2];
            for (int i = input.Length-1; i >= 0; i--)
            {
                var t = BitConverter.GetBytes(input[i]);
                tmp[0] = t[1];
                tmp[1] = t[0];
                list.AddRange(tmp);
            }

            return list.ToArray();
        }
        /// <summary>
        /// 通过转换得到待发送的B值数据包
        /// </summary>
        /// <param name="bfloats">各通道B值</param>
        /// <returns>一包bytes数据</returns>
        public byte[] GetSendBvalues(float[] bfloats)
        {
            List<List<byte>> blist = new List<List<byte>>();
            for (int i = 0; i < 8; i++)
            {
                blist.Add(new List<byte>());
            }

            for (int i = 0; i < bfloats.Length; i++)
            {
                short x;
                if (bfloats[i] < 0.271)
                {
                    x = 32767;
                }
                else if (bfloats[i] > 17712)
                {
                    x = 0;
                }
                else
                {
                    x = (short)(8856 / bfloats[i]);
                }

                var tmp = BitConverter.GetBytes(x);

                blist[i % 8].AddRange(tmp);
            }
            var package = new List<byte>();
            short t = 32767;
            for (int i = 0; i < 24; i++)
            {
                var tmp = BitConverter.GetBytes(t);
                package.AddRange(tmp);
            }

            for (int i = 0; i < 8; i++)
            {
                blist[i].AddRange(package);
            }
            List<byte> zList = new List<byte>();
            for (int i = 0; i < blist.Count; i++)
            {
                zList.AddRange(blist[i].ToArray());
            }
            return zList.ToArray();
        }

        #region IDisposable

        void ReleaseUnmanagedResources() {
            // TODO release unmanaged resources here
        }

        void Dispose(bool disposing) {
            ReleaseUnmanagedResources();
            if (disposing) {
                if (RcvResetEvent != null) RcvResetEvent.Dispose();
                if (MSocket != null) MSocket.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~WaveSocket() {
            Dispose(false);
        }

        #endregion
    }

    
}
