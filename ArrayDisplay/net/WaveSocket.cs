// 2019010710:15 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ArrayDisplay.UI;

namespace ArrayDisplay.net {
    public class WaveSocket:IDisposable  {
        public byte[] RcvBuf {
            get;
            private set;
        }

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

        public  AutoResetEvent RcvResetEvent;
        public WaveSocket() {
            MSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                    ProtocolType.Udp);
        }

        public bool StartReceiveData(IPEndPoint ip) {
            try {
                MSocket.Blocking = true;
                MSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH;
                RcvBuf = new byte[ConstUdpArg.ORIG_FRAME_LENGTH*100];
                
                MSocket.Bind(ip);
                RcvThread = new Thread(RcvUdpdata) {IsBackground = true};
                RcvResetEvent = new AutoResetEvent(false);
                BvalueData = new short[64][];
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
            finally {
                RcvThread.Start();
            }
            return true;

        }

        void RcvUdpdata() {
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>(2) {
                new ArraySegment<byte>(new byte[1282]),
            };
            int ret;
            Stopwatch stopwatch = new Stopwatch();
            while(true) {
                RcvResetEvent.WaitOne();
                try {
                    stopwatch.Start();
                    ret = MSocket.Receive(buffer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (ret <= 0)
                {
                    return;
                }
                int offset = 0;
                var bytedata = buffer[0].Array;;
                var head = new byte[2];
                Array.Copy(bytedata, head, head.Length);
                var tiv = head[0];
                var chl = head[1];
                int index = chl * 8 + tiv;
                offset += 2;
                short[] sbdata = new short[(bytedata.Length - offset) / 2];
                var purdata = new byte[bytedata.Length - offset];
                Array.Copy(bytedata, offset, purdata, 0,purdata.Length);
                for (int i = 0; i < sbdata.Length; i++)
                {
                    var t = new byte[2];
                    t[0] = bytedata[2*i + 1];
                    t[1] = bytedata[2*i];
                    sbdata[i] = BitConverter.ToInt16(t, 0);
                }
                BvalueData[index] = new short[sbdata.Length];
                double progressvalue =(index + 1) * 100 / 64;
                Console.WriteLine("通道： "+index);
                Array.Copy(sbdata, BvalueData[index], sbdata.Length);
                stopwatch.Stop();
                Console.WriteLine("RCVTime:"+stopwatch.ElapsedMilliseconds);
                DisPlayWindow.hMainWindow.bvaulue_pgbar.Dispatcher.Invoke(() => {
                                                                              DisPlayWindow.hMainWindow.bvaulue_pgbar.Value = progressvalue;
                                                                          });
                RcvResetEvent.Reset();
            }
            
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
