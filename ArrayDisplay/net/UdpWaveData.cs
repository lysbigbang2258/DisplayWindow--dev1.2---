﻿using System;
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

        /// <summary>
        ///     开启接收接收数据
        /// </summary>
        /// <param name="ip"></param>
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
            FrameNums = DisPlayWindow.Info.OrigFramNums;
            rcvBuf = new byte[ConstUdpArg.ORIG_FRAME_LENGTH];
            RcvThread = new Thread(OrigThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Orig"};
        }

        void DelayInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.DELAY_FRAME_NUMS * ConstUdpArg.DELAY_FRAME_LENGTH * 2;
            FrameNums = ConstUdpArg.DELAY_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.DELAY_FRAME_LENGTH];
            RcvThread = new Thread(DelayThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Delay"};
        }

        void WorktInit() {
            waveSocket.ReceiveBufferSize = ConstUdpArg.WORK_FRAME_LENGTH * ConstUdpArg.WORK_FRAME_NUMS * 2;
            FrameNums = ConstUdpArg.WORK_FRAME_NUMS;
            rcvBuf = new byte[ConstUdpArg.WORK_FRAME_LENGTH * 2];
            RcvThread = new Thread(NormalThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "WorkWave"};
        }

        void DelayThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");

            while(true) {
                while(true) {
                    StartRcvEvent.WaitOne();
                    int index = 0;
                    IsRcving = true;
                    while(index < FrameNums) {
                        if (waveSocket == null) {
                            IsRcving = false;
                            break;
                        }

                        int offset = 0;
                        try {
                            //接收数据

                            int ret = waveSocket.ReceiveFrom(rcvBuf, offset, rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                            IsRcving = false;
                            break;
                        }

                        switch(WaveType) {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler != null) {
                                    WorkSaveDataEventHandler(null, rcvBuf);
                                }
                                PutWorkData(rcvBuf, index++);
                                if (index >= FrameNums) {
                                    index = 0;
                                }
                                waveDataproc.WorkBytesEvent.Set();
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                PutOrigData(rcvBuf);
                                waveDataproc.OrigBytesEvent.Set();
                                index++;
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }
                                break;
                            case ConstUdpArg.WaveType.Delay:
                                PutDelayData(rcvBuf);
                                index++;
                                if (index >= FrameNums) {
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

        void OrigThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");
            while(true) {
                while(true) {
                    StartRcvEvent.WaitOne();
                    int index = 0;
                    IsRcving = true;
                    while(index < FrameNums) {
                        if (waveSocket == null) {
                            IsRcving = false;
                            break;
                        }
                        int offset = 0;
                        try {
                            //接收数据                         
                                int ret = waveSocket.ReceiveFrom(rcvBuf, offset, rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                            IsRcving = false;
                            break;
                        }

                        switch(WaveType) {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler != null) {
                                    WorkSaveDataEventHandler(null, rcvBuf);
                                }
                                PutWorkData(rcvBuf, index++);
                                if (index >= FrameNums) {
                                    index = 0;
                                }
                                waveDataproc.WorkBytesEvent.Set();
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                PutOrigData(rcvBuf);
                                waveDataproc.OrigBytesEvent.Set();
                                index++;
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }
                                break;
                            case ConstUdpArg.WaveType.Delay:
                                PutDelayData(rcvBuf);
                                index++;
                                if (index >= FrameNums) {
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

        void NormalThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");
            while(true) {
                StartRcvEvent.WaitOne();
                if (IsStopRcved) {
                    return;
                }
                int index = 0;
                IsRcving = true;
                while(index < FrameNums) {
                    if (waveSocket == null) {
                        IsRcving = false;
                        break;
                    }

                    int offset = 0;
                    try {
                        int ret = waveSocket.ReceiveFrom(rcvBuf, offset, rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        if (!Equals(senderRemote, ConstUdpArg.Dst_NormWaveIp)) {
                            Console.WriteLine("接收错误");
                            Console.WriteLine(senderRemote.ToString());
                            continue;
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        break;
                    }

                    if (WorkSaveDataEventHandler != null) {
                        WorkSaveDataEventHandler(null, rcvBuf);
                    }
                    PutWorkData(rcvBuf, index++);
                    if (index >= FrameNums) {
                        index = 0;
                    }
                    waveDataproc.WorkBytesEvent.Set();
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
            if (channel > 7 || channel < 0) {
                return;
            }
            offset += head.Length;
            Array.Copy(buf, offset, temp, 0, temp.Length);
            if (delayChannelOffsets[channel] >= waveDataproc.DelayWaveBytes[0].Length) {
                delayChannelOffsets[channel] = 0;
            }

            Array.Copy(temp, 0, waveDataproc.DelayWaveBytes[channel], delayChannelOffsets[channel], temp.Length);
            delayChannelOffsets[channel] += temp.Length;
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

            int len = origChannelOffsets[channel  + timdiv * 8];//写入数据偏移长度
            if (len >= waveDataproc.OrigWaveBytes[0].Length) { //大于容量数据覆盖，从头再写入
                origChannelOffsets[channel + timdiv * 8] = 0;
                len = 0;
            }
            Array.Copy(buf, offset, waveDataproc.OrigWaveBytes[channel + timdiv* 8 ], len, buf.Length - 2);

            var data = new byte[buf.Length - 2];
            Array.Copy(buf, offset, data, 0, buf.Length - 2);
            origChannelOffsets[channel * 8 + timdiv] += data.Length;
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
            var dataBytes = new byte[1024];
            Array.Copy(buf, dataBytes, dataBytes.Length);
            for(int i = 0; i < (dataBytes.Length / 4); i++) {
                Array.Copy(dataBytes, i * 4, temp, 0, temp.Length);
                Array.Copy(temp, 0, waveDataproc.WorkWaveBytes[i], index * 4, temp.Length);
            }
        }

        #region Field

        static readonly LinkedList<Array> linkbuffer = new LinkedList<Array>(); //缓存数据buff
        public static int FrameNums; //一帧数据长度
        public static ConstUdpArg.WaveType waveType; //波形数据类型
        readonly int[] delayChannelOffsets = new int[8];
        readonly int[] origChannelOffsets = new int[64];
        byte[] rcvBuf; //接收数据缓存
        readonly Socket waveSocket;
        Dataproc waveDataproc;

        #endregion

        #region 属性

        public bool IsBuilded {
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

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (waveSocket != null) {
                    waveSocket.Shutdown(SocketShutdown.Both);
                    waveSocket.Close();
                    waveSocket.Dispose();
                }
                if (waveDataproc != null) {
                    waveDataproc.Dispose();
                }
                if (StartRcvEvent != null) {
                    StartRcvEvent.Dispose();
                }
                if (StartRcvEvent != null) {
                    StartRcvEvent.Dispose();
                }
                if (RcvThread != null) {
                    RcvThread.Abort();
                    RcvThread = null;
                }
                IsBuilded = false;
                IsStopRcved = true;
                linkbuffer.Clear();
                Console.WriteLine(@"关闭UDP线程...");
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
