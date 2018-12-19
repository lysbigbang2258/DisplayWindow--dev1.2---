using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using NationalInstruments.Controls;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace ArrayDisplay.net {
    public class UdpWaveData:IDisposable{
        #region Field

        public static bool isBuilded; //UDP是否创建
        static LinkedList<Array> linkbuffer = new LinkedList<Array>();//缓存数据buff
//        public static EventHandler<byte[]> divDataEventHandler;
        public static bool isReceived;
        public static int frameNums; //一帧数据长度
        public static ConstUdpArg.WaveType waveType;//波形数据类型
        int[] delaychannelOffsets = new int[8];
        int[] origchannelOffsets = new int[64];
        public byte[][] detesBytes;
        byte[] rcvBuf;//接收数据缓存
        readonly Socket waveSocket;
        public Dataproc wavaDataproc;
        public byte[] saveBytes; 
        #endregion

        #region 属性

        public Thread Normalthread {
            get;
            private set;
        }

        public Thread Originthread {
            get;
            private set;
        }

        public IPEndPoint Ip {
            get;
            private set;
        }
        /// <summary>
        /// 正常工作数据保存方法
        /// </summary>
        public static EventHandler<byte[]> WorkSaveDataEventHandler
        {
            get;
            set;
        }
        /// <summary>
        /// 原始数据保存方法
        /// </summary>
        public static EventHandler<byte[]> OrigSaveDataEventHandler
        {
            get;
            set;
        }
        /// <summary>
        /// 正在传输波形
        /// </summary>
        public static ConstUdpArg.WaveType WaveType {
            get {
                return waveType;
            }
            set {
                waveType = value;
            }
        }

        #endregion

        public UdpWaveData(IPEndPoint ip) {
            Ip = ip;
            wavaDataproc = new Dataproc();
            saveBytes = new byte[ConstUdpArg.WORK_FRAME_LENGTH *
                                 ConstUdpArg.WORK_FRAME_NUMS ];
            try {
                waveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                        ProtocolType.Udp);
                waveSocket.Bind(ip);
                isBuilded = true;
            }
            catch(Exception e) {
                Console.WriteLine(@"创建UDP失败...错误为{0}", e);
                MessageBox.Show(@"创建UDP失败...");
            }
            if (Equals(ip, ConstUdpArg.SrcNorm_WaveIp)) {
                WaveSocketInit();
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
            Normalthread.Start();
        }

        void OrigInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH *
                                           ConstUdpArg.ORIG_FRAME_NUMS * 2;
            frameNums = ConstUdpArg.ORIG_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.ORIG_FRAME_LENGTH];
            Normalthread = new Thread(WaveThreadStart) {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "Orig"
            };
        }

        void DelayInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.DELAY_FRAME_NUMS *
                                           ConstUdpArg.DELAY_FRAME_LENGTH * 2;
            frameNums = ConstUdpArg.DELAY_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.DELAY_FRAME_LENGTH];
            Normalthread = new Thread(WaveThreadStart) {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "Delay"
            };
        }

        void WaveSocketInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.WORK_FRAME_LENGTH *
                                           ConstUdpArg.WORK_FRAME_NUMS * 2;
            frameNums = ConstUdpArg.WORK_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.WORK_FRAME_LENGTH];
            Normalthread = new Thread(WaveThreadStart) {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "WorkWave"
            };
        }

        void WaveThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");
            Stopwatch watch = new Stopwatch();
            try {
                while(isBuilded) {
                    int index = 0;
                    while(index < frameNums) {
                        int offset = 0;
                        try {
                            //接收数据
                            int ret = waveSocket.ReceiveFrom(rcvBuf, offset,
                                                             rcvBuf.Length - offset,
                                                             SocketFlags.None, ref senderRemote);
                            if (Equals(senderRemote, ConstUdpArg.Dst_WorkDatIp)) offset += ret;
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                            throw;
                        }
                        switch(WaveType) {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler!=null) {
                                    WorkSaveDataEventHandler(null, rcvBuf);
                                }
                                PutWorkData(rcvBuf, index++);
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                
                                PutOrigData(rcvBuf);
                                index++;
                                break;
                            case ConstUdpArg.WaveType.Delay:
                                PutDelayData(rcvBuf);
                                index++;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    switch (WaveType)
                    {
                        case ConstUdpArg.WaveType.Normal:
                            wavaDataproc.WorkBytesEvent.Set();
                            break;
                        case ConstUdpArg.WaveType.Orig:
                            wavaDataproc.OrigBytesEvent.Set();
                            break;
                        case ConstUdpArg.WaveType.Delay:                            
                            wavaDataproc.DelayBytesEvent.Set();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
//                    Console.WriteLine(@"Receive frame time = {0:f3}", watch.ElapsedMilliseconds / 1000.0f);
                    watch.Stop();

                }
            }
            catch(ThreadAbortException) {
                isBuilded = false;
                waveSocket.Close();
                isReceived = false;
                Console.WriteLine(@"关闭Socket...");
            }
        }

        void PutDelayData(byte[] buf) {
            var temp = new byte[400];
            var head = new byte[2];

            int offset = 0;
            if (!Equals(Ip, ConstUdpArg.Src_DelayWaveIp)) return;
            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[1];
            offset += head.Length;
            Array.Copy(buf, offset, temp, 0, temp.Length);
            Array.Copy(temp, 0, wavaDataproc.DelayWaveBytes[channel], delaychannelOffsets[channel],
                       temp.Length);
            delaychannelOffsets[channel] += temp.Length;
        }
        /// <summary>
        /// 将原始数据保存到对应通道
        /// 发送数据到保存线程
        /// </summary>
        /// <param name="buf"></param>
        void PutOrigData(byte[] buf) {
            var head = new byte[2];
            int offset = 0;
            if (!Equals(Ip, ConstUdpArg.Src_OrigWaveIp)) return;
            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[0];
            int timdiv = head[1];
            if (channel < 0 && channel > ConstUdpArg.ORIG_CHANNEL_NUMS) {
                channel = 1;
            }
            if (timdiv < 0 && timdiv > ConstUdpArg.ORIG_TIME_NUMS)
            {
                timdiv = 1;
            }
            offset += head.Length;
            byte[] data = new byte[buf.Length-2];
            Array.Copy(buf, offset,data , 0, buf.Length-2);
            var len = origchannelOffsets[channel * 8 + timdiv];
            if (len >= wavaDataproc.OrigWaveBytes[0].Length)
            {
                origchannelOffsets[channel * 8 + timdiv] = 0;
                len = 0;
            }
            Array.Copy(buf, offset, wavaDataproc.OrigWaveBytes[channel * 8 + timdiv], len, buf.Length - 2);
           
            origchannelOffsets[channel * 8 + timdiv] += data.Length;
            if (OrigSaveDataEventHandler!=null) {    //发送给保存线程
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
            if (!Equals(Ip, ConstUdpArg.SrcNorm_WaveIp)) return;
            for(int i = 0; i < (buf.Length / 4); i++) {
                Array.Copy(buf, i * 4, temp, 0, temp.Length);
                Array.Copy(temp, 0, wavaDataproc.WorkWaveBytes[i], index * 4, temp.Length);
            }
        }

        public void Close() {
            isBuilded = false;
            isReceived = false;
            linkbuffer.Clear();
            Console.WriteLine(@"关闭UDP线程...");
            if (Normalthread == null) return;
            Normalthread.Abort();
            Normalthread = null;
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (waveSocket != null) waveSocket.Dispose();
                if (wavaDataproc != null) wavaDataproc.Dispose();
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
