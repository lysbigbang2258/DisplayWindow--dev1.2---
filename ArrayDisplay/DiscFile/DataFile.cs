using System;
using System.Collections.Concurrent;
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
        readonly int origLength;
        readonly ConcurrentQueue<byte[]> origRcvQueue;
        readonly AutoResetEvent origResetEvent;
        readonly ConcurrentQueue<byte[]> workRcvQueue;
        readonly AutoResetEvent workResetEvent;
        BinaryWriter br_work;

        string filename;
        FileStream fs_work;
        Thread oriThread;
        int packnum;
        int workLength;
        Thread workThread;

        #endregion

        #region Property

        public bool IsStartFlag {
            get;
            set;
        }

        #endregion

        #region Method

        public DataFile() {
            origRcvQueue = new ConcurrentQueue<byte[]>();
            workRcvQueue = new ConcurrentQueue<byte[]>();
            origResetEvent = new AutoResetEvent(false);
            workResetEvent = new AutoResetEvent(false);
            RelativeDirectory rd = new RelativeDirectory();
            origLength = ConstUdpArg.SAVE_ORIGPACK;
            workLength = ConstUdpArg.SAVE_WORKPACK;
            filepath = rd.Path + "\\" + "wavedata" + "\\";
            IsStartFlag = false;
            packnum = 0;
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
        ///     关闭数据保存
        /// </summary>
        public void DisableSaveFile() {
            if (oriThread != null && oriThread.IsAlive) {
                origResetEvent.Reset();
                oriThread.Abort();
            }
            if (workThread != null && workThread.IsAlive) {
                workThread.Abort();
            }
        }

        #region orig相关

        /// <summary>
        ///     线程处理：写入原始数据
        /// </summary>
        void Thread_OrigDataSave() {
            try {
                while(true) {
                    origResetEvent.WaitOne();
                    if (origRcvQueue.Count < origLength) {
                        continue;
                    }
                    try {
                        using(FileStream fsOrig = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                                byte[] temp;
                                int length = 0;
                                for(int i = 0; i <= origLength; i++) {
                                    origRcvQueue.TryDequeue(out temp);
                                    if (temp != null) {
                                        brWork.Write(temp);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                    origResetEvent.Set();
                }
            }
            catch(ThreadAbortException abortException) {
                Thread.ResetAbort();
                if (!File.Exists(filename)) { return; }
                try {
                    using(FileStream fsOrig = new FileStream(filename, FileMode.Append, FileAccess.Write)) {
                        using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                            byte[] temp;
                            for(int i = 0; i <= origRcvQueue.Count; i++) {
                                origRcvQueue.TryDequeue(out temp);
                                if (temp != null) {
                                    brWork.Write(temp);
                                }
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     开启原始数据保存
        /// </summary>
        public void EnableOrigSaveFile() {
            oriThread = new Thread(Thread_OrigDataSave) {IsBackground = true};
            oriThread.Start();
            UdpWaveData.OrigSaveDataEventHandler += WriteOrigData;
        }

        /// <summary>
        ///     实时获取原始数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void WriteOrigData(object sender, byte[] data) {
            origRcvQueue.Enqueue(data);
            if (!IsStartFlag) {
                return;
            }
            string str = SetNowTimeStr(WaveData.Origdata);
            filename = filepath + str;
            origResetEvent.Set();
        }

        #endregion

        #region work相关

        void Thread_WorkDataSave() {
            try {
                while(true) {
                    workResetEvent.WaitOne();
                    if (workRcvQueue.Count < workLength)
                    {
                        continue;
                    }
                    try {
                        using(FileStream fsOrig = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                                byte[] temp;
                                for(int i = 0; i <= workLength; i++) {
                                    workRcvQueue.TryDequeue(out temp);
                                    if (temp != null) {
                                        brWork.Write(temp);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            catch(ThreadAbortException abortException) {
                Thread.ResetAbort();
                if (!File.Exists(filename)) { return; }
                try {
                    using(FileStream fsOrig = new FileStream(filename, FileMode.Append, FileAccess.Write)) {
                        using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                            byte[] temp;
                            for(int i = 0; i <= workRcvQueue.Count; i++) {
                                workRcvQueue.TryDequeue(out temp);
                                if (temp != null) {
                                    brWork.Write(temp);
                                }
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     开启工作数据保存
        /// </summary>
        public void EnableWorkSaveFile() {
            workThread = new Thread(Thread_WorkDataSave) {IsBackground = true};
            workThread.Start();
            UdpWaveData.WorkSaveDataEventHandler += WriteWorkData;
        }
        /// <summary>
        /// 写入工作数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void WriteWorkData(object sender, byte[] data) {
            workRcvQueue.Enqueue(data);
            if (!IsStartFlag)
            {
                return;
            }
            string str = SetNowTimeStr(WaveData.WorkData);
            filename = filepath + str;
            workResetEvent.Set();
        }

        #endregion

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

        #endregion
    }
}
