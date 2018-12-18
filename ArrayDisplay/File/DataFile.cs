using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArrayDisplay.net;
using ArrayDisplay.UI;

namespace ArrayDisplay.File {
    public class DataFile : IDisposable {
        readonly List<byte[]> workSavelist; //保存接收数据
        readonly List<byte[]> origSavelist; //保存接收数据
        ConcurrentQueue<byte[]> origRcvQueue;
        ConcurrentQueue<byte[]> origReadQueue;
        readonly string filepath;
        readonly Semaphore sem_work;
        readonly Semaphore sem_orig;
        readonly AutoResetEvent origResetEvent;
        BinaryWriter br_work;
        BinaryWriter br_orig;
        FileStream fs_work;
        FileStream fs_orig;

        public DataFile() {
            workSavelist = new List<byte[]>(ConstUdpArg.WORK_FRAME_NUMS * 50);
            origSavelist = new List<byte[]>(ConstUdpArg.ORIG_FRAME_NUMS * 50);
            origRcvQueue = new ConcurrentQueue<byte[]>();
            origReadQueue = new ConcurrentQueue<byte[]>();
            sem_work = new Semaphore(0, 10);
            sem_orig = new Semaphore(0, 10);
            origResetEvent = new AutoResetEvent(false);
            Thread hthread = new Thread(Thread_WorkDataSave) {IsBackground = true};
            hthread.Start();
            RelativeDirectory rd = new RelativeDirectory();
            filepath = rd.Path + "\\";
            Task.Factory.StartNew(Thread_OrigDataSave);

        }

        void Thread_WorkDataSave() {
            while(true) {
                sem_work.WaitOne();
                if (fs_work != null) continue;

                DateTime dt = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                sb.Append("file_");
                sb.Append(DisPlayWindow.SelectdInfo.WorkWaveChannel.ToString("d3"));
                sb.Append("_");
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
                sb.Append(".bin");
                if (workSavelist.Count >= (ConstUdpArg.WORK_FRAME_NUMS * 5)) {
                    fs_work = new FileStream(filepath + sb, FileMode.Create, FileAccess.Write);
                    br_work = new BinaryWriter(fs_work);

                    for(int i = 0; i < (ConstUdpArg.WORK_FRAME_NUMS * 5); i++)
                        br_work.Write(workSavelist[i]); // 保存数据于磁盘
                    workSavelist.RemoveRange(0, ConstUdpArg.WORK_FRAME_NUMS * 5 - 1);
                    br_work.Flush();
                    br_work.Close();
                    fs_work.Close();
                    fs_work = null;
                }
            }
        }

        void Thread_OrigDataSave()
        {
            while (true)
            {
//                sem_orig.WaitOne();
                origResetEvent.WaitOne();
               

                DateTime dt = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                sb.Append("Origfile_");
//                sb.Append("Chl:");
//                sb.Append(DisPlayWindow.hMainWindow.tb_origChannel.Text);
//                sb.Append("Div:");
//                sb.Append(DisPlayWindow.hMainWindow.tb_orgiTdiv.Text);
//                sb.Append("_");
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
                sb.Append(".bin");
                var str = sb.ToString();

                if (origRcvQueue.Count >= 1024*10)
                {
                    try {
                        fs_orig = new FileStream(filepath + str, FileMode.CreateNew, FileAccess.Write);
                        br_orig = new BinaryWriter(fs_orig);
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }

                    byte[] temp ;
                    for (int i = 0; i <= 1024*10; i++)
                    {
                        origRcvQueue.TryDequeue(out temp);
                        br_orig.Write(temp);
                    }
                    origResetEvent.Set();
//                    br_orig.Flush();
//                    br_orig.Close();
//                    fs_orig.Close();
//                    fs_orig = null;
                }
            }
        }

        public void EnableWorkSaveFile() {
            UdpWaveData.WorkSaveDataEventHandler += WriteOrigData;
        }
        public void EnableOrigSaveFile()
        {
            UdpWaveData.OrigSaveDataEventHandler += WriteOrigData;
        }

        public void DisableSaveFile() {
            UdpWaveData.WorkSaveDataEventHandler = null;
        }

        void WriteSaveData(object sender, byte[] data) {
            var buffer = new byte[data.Length];
            Array.Copy(data, 0, buffer, 0, data.Length);
            if (workSavelist.Count < (ConstUdpArg.WORK_FRAME_NUMS * 50)) {
                workSavelist.Add(buffer);
                if (0 == workSavelist.Count % ConstUdpArg.WORK_FRAME_NUMS * 5)
                    sem_work.Release();
            }
        }

        void WriteOrigData(object sender, byte[] data)
        {
            origRcvQueue.Enqueue(data);
            origResetEvent.Set();
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (fs_work != null) fs_work.Dispose();
                if (br_work != null) br_work.Dispose();
                if (sem_work != null) sem_work.Dispose();
                if (fs_orig != null) fs_orig.Dispose();
                if (br_orig != null) br_orig.Dispose();
                if (sem_orig != null) sem_orig.Dispose();
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
