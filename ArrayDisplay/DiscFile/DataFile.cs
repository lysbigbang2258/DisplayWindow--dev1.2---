using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ArrayDisplay.DataFile;
using ArrayDisplay.net;

namespace ArrayDisplay.DiscFile {
    public enum WaveData {
        WorkData,
        Origdata
    }

    public class DataFile : IDisposable {
        #region Field

        readonly string filepath;
        readonly ConcurrentQueue<byte[]> origRcvQueue;
        readonly AutoResetEvent origResetEvent;
        readonly ConcurrentQueue<byte[]> workRcvQueue;
        readonly AutoResetEvent workResetEvent;
        BinaryWriter br_work;
        FileStream fs_work;
        readonly int origLength;
        Thread oriThread;
        int workLength;
        Thread workThread;

        string filename;
        int packnum;
        #endregion

        public bool IsStartFlag {
            get;
            set;
        }

        public DataFile() {
            origRcvQueue = new ConcurrentQueue<byte[]>();
            workRcvQueue = new ConcurrentQueue<byte[]>();
            origResetEvent = new AutoResetEvent(false);
            workResetEvent = new AutoResetEvent(false);
            RelativeDirectory rd = new RelativeDirectory();
            origLength = ConstUdpArg.SAVE_ORIGPACK;
            workLength = ConstUdpArg.SAVE_WORKPACK;
            filepath = rd.Path + "\\"+"wavedata"+"\\";
            IsStartFlag = false;
            packnum = 0;
        }

        void Thread_WorkDataSave() {
            while(true) {
                workResetEvent.WaitOne();
                string str = SetNowTimeStr(WaveData.WorkData);
                if (workRcvQueue.Count >= (1024 * 100 * 4)) {
                    try {
                        fs_work = new FileStream(filepath + str, FileMode.CreateNew, FileAccess.Write);
                        br_work = new BinaryWriter(fs_work);
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                    byte[] temp;
                    for(int i = 0; i <= (1024 * 100 * 4); i++) {
                        workRcvQueue.TryDequeue(out temp);
                        br_work.Write(temp);
                    }
                    workResetEvent.Set();
                }
            }
        }

        /// <summary>
        /// 线程处理：写入原始数据
        /// </summary>
        void Thread_OrigDataSave() {
            try {
                while (true)
                {
                    origResetEvent.WaitOne();
                    if (origRcvQueue.Count < origLength) {
                        continue;
                    }
                    try
                    {
                        using (var fsOrig = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            using (var brWork = new BinaryWriter(fsOrig))
                            {
                                byte[] temp;
                                int length = 0;
                                for (int i = 0; i <= origLength; i++)
                                {
                                    origRcvQueue.TryDequeue(out temp);
                                    if (temp != null)
                                    {
                                        brWork.Write(temp);
                                    }
                                }
                            }
                        }  

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                origResetEvent.Set();
                }
                
            }
            catch(ThreadAbortException abortException) {
                Thread.ResetAbort();
                if (!File.Exists(filename)) {return; }
                try
                {
                    using (var fsOrig = new FileStream(filename, FileMode.Append, FileAccess.Write))
                    {
                        using (var brWork = new BinaryWriter(fsOrig))
                        {
                            byte[] temp;
                            for (int i = 0; i <= origRcvQueue.Count; i++)
                            {
                                origRcvQueue.TryDequeue(out temp);
                                if (temp != null)
                                {
                                    brWork.Write(temp);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
           
        }

        /// <summary>
        ///     获取当前时间信息
        /// </summary>
        /// <returns></returns>
        static string SetNowTimeStr(WaveData wave) {
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(wave == WaveData.WorkData ? "WorkFile_" : "OrigFile_");
            sb.Append(dt.Year.ToString("d4"));
            sb.Append("-");
            sb.Append(dt.Month.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Day.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Hour.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Minute.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Second.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Millisecond.ToString("d2"));
            sb.Append(".bin");
            string str = sb.ToString();
            return str;
        }
        /// <summary>
        /// 开启工作数据保存
        /// </summary>
        public void EnableWorkSaveFile() {
            workThread = new Thread(Thread_WorkDataSave) { IsBackground = true };
            workThread.Start();
            UdpWaveData.WorkSaveDataEventHandler += WriteWorkData;
        }
        /// <summary>
        /// 开启原始数据保存
        /// </summary>
        public void EnableOrigSaveFile() {
            oriThread = new Thread(Thread_OrigDataSave) { IsBackground = true };
            oriThread.Start();
            UdpWaveData.OrigSaveDataEventHandler += WriteOrigData;
            origResetEvent.Set();
        }

        public void DisableSaveFile() {
            if (oriThread!=null && oriThread.IsAlive) {
                origResetEvent.Reset();
                oriThread.Abort();
            }
            if (workThread!=null && workThread.IsAlive)
            {
                workThread.Abort();
            }
        }

        void WriteWorkData(object sender, byte[] data) {
            workRcvQueue.Enqueue(data);
            int len = ConstUdpArg.SAVE_WORKPACK;
            if (workRcvQueue.Count >= len) {
                string filename = SetNowTimeStr(WaveData.WorkData);
            }
        }

        /// <summary>
        /// 实时获取原始数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void WriteOrigData(object sender, byte[] data) {
            packnum++;
            origRcvQueue.Enqueue(data);
            if (IsStartFlag) {
                string str = SetNowTimeStr(WaveData.Origdata);
                filename = filepath + str;
                origResetEvent.Set();
                
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (fs_work != null) {
                    fs_work.Dispose();
                }
                if (br_work != null) {
                    br_work.Dispose();
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
