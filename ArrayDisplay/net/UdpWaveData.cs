using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using ArrayDisplay.UI;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace ArrayDisplay.net {
    public class UdpWaveData : IDisposable {
        public UdpWaveData() {
            try {
                waveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                StartRcvEvent = new AutoResetEvent(false);
                IsStopRcved = false;
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        public void StartReceiveData(IPEndPoint ip) {
            Ip = ip;        
            try {
                waveSocket.Bind(ip);
                IsBuilded = true;
            }
            catch(Exception e) {
                Console.WriteLine(@"创建UDP失败...错误为{0}", e);
                MessageBox.Show(@"创建UDP失败...");
            }
            if (Equals(ip, ConstUdpArg.Src_NormWaveIp)) {
                WorktInit();
                WaveType = ConstUdpArg.WaveType.Normal;
            }
            else if (Equals(ip, ConstUdpArg.Src_OrigWaveIp)) {
                OrigInit();
                WaveType = ConstUdpArg.WaveType.Orig;
            }
            else if (Equals(ip, ConstUdpArg.Src_DelayWaveIp)) {
                DelayInit();
                WaveType = ConstUdpArg.WaveType.Delay;
            }
            waveDataproc = new Dataproc();
            waveDataproc.Init(WaveType);
            RcvThread.Start();
        }

        public void StopReceiveData() {
            StartRcvEvent.Reset();
            RcvThread.Abort();
        }

        public void RefreshReceiveData() {
           
            StartRcvEvent.Set();
             
        }

        void OrigInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH * ConstUdpArg.ORIG_FRAME_NUMS * 2;
            frameNums = DisPlayWindow.SelectdInfo.OrigFramNums;
            rcvBuf = new byte[ConstUdpArg.ORIG_FRAME_LENGTH];
            RcvThread = new Thread(WaveThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Orig"};
        }

        void DelayInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.DELAY_FRAME_NUMS * ConstUdpArg.DELAY_FRAME_LENGTH * 2;
            frameNums = ConstUdpArg.DELAY_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.DELAY_FRAME_LENGTH];
            RcvThread = new Thread(WaveThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Delay"};
        }

        void WorktInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.WORK_FRAME_LENGTH * ConstUdpArg.WORK_FRAME_NUMS * 2;
            frameNums = ConstUdpArg.WORK_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.WORK_FRAME_LENGTH];
            RcvThread = new Thread(WaveThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "WorkWave"};
        }

        void WaveThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");

            while(true) {
                while(true) {
                    StartRcvEvent.WaitOne();
                    int index = 0;
                    IsRcving = true;
                    while (index < frameNums)
                    {
                        if (waveSocket == null) {
                            IsRcving = false;
                            break;
                        }

                        int offset = 0;
                        try
                        {
                            //接收数据
                            
                            int ret = waveSocket.ReceiveFrom(rcvBuf, offset, rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            IsRcving = false;
                            break;
                        }

                        switch (WaveType)
                        {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler != null)
                                {
                                    WorkSaveDataEventHandler(null, rcvBuf);
                                }
                                PutWorkData(rcvBuf, index++);
                                if (index >= frameNums)
                                {
                                    index = 0;
                                }
                                waveDataproc.WorkBytesEvent.Set();
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                PutOrigData(rcvBuf);
                                waveDataproc.OrigBytesEvent.Set();
                                index++;
                                index++;
                                if (index >= frameNums)
                                {
                                    index = 0;
                                }
                                break;
                            case ConstUdpArg.WaveType.Delay:
                                PutDelayData(rcvBuf);
                                index++;
                                if (index >= frameNums)
                                {
                                    index = 0;
                                }
                                waveDataproc.DelayBytesEvent.Set();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                
            }
        }

        

        void PutDelayData(byte[] buf) {
            var temp = new byte[400];
            var head = new byte[2];

            int offset = 0;
            if (!Equals(Ip, ConstUdpArg.Src_DelayWaveIp)) {
                return;
            }
            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[1];
            offset += head.Length;
            Array.Copy(buf, offset, temp, 0, temp.Length);
            if (delaychannelOffsets[channel] >= waveDataproc.DelayWaveBytes[0].Length) {
                delaychannelOffsets[channel] = 0;
            }

            Array.Copy(temp, 0, waveDataproc.DelayWaveBytes[channel], delaychannelOffsets[channel], temp.Length);
            delaychannelOffsets[channel] += temp.Length;
        }

        /// <summary>
        ///     将原始数据保存到对应通道
        ///     发送数据到保存线程
        /// </summary>
        /// <param name="buf"></param>
        void PutOrigData(byte[] buf) {
            var head = new byte[2];
            int offset = 0;
            if (!Equals(Ip, ConstUdpArg.Src_OrigWaveIp)) {
                return;
            }
            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[0];
            int timdiv = head[1];
            if (channel < 0 && channel > ConstUdpArg.ORIG_CHANNEL_NUMS) {
                channel = 1;
            }
            if (timdiv < 0 && timdiv > ConstUdpArg.ORIG_TIME_NUMS) {
                timdiv = 1;
            }
            offset += head.Length;

            int len = origchannelOffsets[channel * 8 + timdiv];
            if (len >= waveDataproc.OrigWaveBytes[0].Length) {
                origchannelOffsets[channel * 8 + timdiv] = 0;
                len = 0;
            }
            Array.Copy(buf, offset, waveDataproc.OrigWaveBytes[channel * 8 + timdiv], len, buf.Length - 2);

            var data = new byte[buf.Length - 2];
            Array.Copy(buf, offset, data, 0, buf.Length - 2);
            origchannelOffsets[channel * 8 + timdiv] += data.Length;
            if (OrigSaveDataEventHandler != null) {
                //发送给保存线程
                OrigSaveDataEventHandler(null, data);
            }
        }

        /// <summary>
        ///     将WorkWave数据导入Detect_Bytes
        /// </summary>
        /// <param name="buf">一帧数据，长度为1024，每4位表示1个探头数据</param>
        /// <param name="index">帧数</param>
        void PutWorkData(byte[] buf, int index) {
            var temp = new byte[4];
            if (!Equals(Ip, ConstUdpArg.Src_NormWaveIp)) {
                return;
            }
            for(int i = 0; i < (buf.Length / 4); i++) {
                Array.Copy(buf, i * 4, temp, 0, temp.Length);
                Array.Copy(temp, 0, waveDataproc.WorkWaveBytes[i], index * 4, temp.Length);
            }
        }

        #region Field

        

        static readonly LinkedList<Array> linkbuffer = new LinkedList<Array>(); //缓存数据buff
        public static int frameNums; //一帧数据长度
        public static ConstUdpArg.WaveType waveType; //波形数据类型
        readonly int[] delaychannelOffsets = new int[8];
        readonly int[] origchannelOffsets = new int[64];
        byte[] rcvBuf; //接收数据缓存
        readonly Socket waveSocket;
        Dataproc waveDataproc;

        #endregion

        #region 属性

        public bool IsBuilded
        {
            get;
            set;
        }

        public bool IsRcving {
            get;
            set;
        }
        public AutoResetEvent StartRcvEvent {
            get;
            set;
        }

        public Thread RcvThread {
            get;
            private set;
        }

        public IPEndPoint Ip {
            get;
            private set;
        }

        public bool IsStopRcved {
            get;
            set;
        }

        /// <summary>
        ///     正常工作数据保存方法
        /// </summary>
        public static EventHandler<byte[]> WorkSaveDataEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     原始数据保存方法
        /// </summary>
        public static EventHandler<byte[]> OrigSaveDataEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     正在传输波形
        /// </summary>
        public ConstUdpArg.WaveType WaveType {
            get {
                return waveType;
            }
            set {
                waveType = value;
            }
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose() {
            if (waveSocket != null) {
                waveSocket.Shutdown(SocketShutdown.Both);
                waveSocket.Dispose();  
            }
            
            if (waveDataproc != null) waveDataproc.Dispose();
            if (StartRcvEvent != null) StartRcvEvent.Dispose();
            if (RcvThread!=null) {
                RcvThread.Abort();
                RcvThread = null;
            }
            IsBuilded = false;

            linkbuffer.Clear();
            Console.WriteLine(@"关闭UDP线程...");
            
        }

        #endregion
    }
}
