using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace ArrayDisplay.net {
    public class UdpWaveData:IDisposable{
        #region Field

        public static bool isRunning;
        public static LinkedList<Array> linkbuffer = new LinkedList<Array>();
//        public static EventHandler<byte[]> divDataEventHandler;
        public static bool isReceived;
        public static int frameNums;
        public static ConstUdpArg.WaveType waveType;
        public int[] channelOffsets = new int[8];
        public byte[][] detesBytes;
        byte[] rcvBuf;
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

        public static EventHandler<byte[]> SaveDataEventHandler
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
                isRunning = true;
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
                while(isRunning) {
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
                                if (SaveDataEventHandler!=null) {
                                    SaveDataEventHandler(null, rcvBuf);
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
                            channelOffsets = new int[8];
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
                isRunning = false;
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
            Array.Copy(temp, 0, wavaDataproc.DelayWaveBytes[channel], channelOffsets[channel],
                       temp.Length);
            channelOffsets[channel] += temp.Length;
        }

        void PutOrigData(byte[] buf) {
            var head = new byte[2];
            int offset = 0;
            if (!Equals(Ip, ConstUdpArg.Src_OrigWaveIp)) return;
            Array.Copy(buf, 0, head, 0, head.Length);
            var channel = head[0];
            var timdiv = head[1];

            offset += head.Length;

            Array.Copy(buf, offset, wavaDataproc.OrigWaveBytes[channel * 8 + timdiv], 0, buf.Length-2);
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
            isRunning = false;
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
