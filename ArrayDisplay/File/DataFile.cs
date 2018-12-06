using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ArrayDisplay.net;
using ArrayDisplay.UI;

namespace ArrayDisplay.File {
    public class DataFile : IDisposable {
        readonly List<byte[]> list; //保存接收数据
        readonly string path;
        readonly Semaphore sem;
        BinaryWriter br;
        FileStream fs;

        public DataFile() {
            list = new List<byte[]>(ConstUdpArg.WORK_FRAME_NUMS * 50);
            sem = new Semaphore(0, 10);
            Thread hthread = new Thread(ThreadSave) {IsBackground = true};
            hthread.Start();
            RelativeDirectory rd = new RelativeDirectory();
            path = rd.Path + "\\" + "data\\";
        }

        void ThreadSave() {
            while(true) {
                sem.WaitOne();
                if (fs != null) continue;

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
                if (list.Count >= (ConstUdpArg.WORK_FRAME_NUMS * 5)) {
                    fs = new FileStream(path + sb, FileMode.Create, FileAccess.Write);
                    br = new BinaryWriter(fs);

                    for(int i = 0; i < (ConstUdpArg.WORK_FRAME_NUMS * 5); i++)
                        br.Write(list[i]); // 保存数据于磁盘
                    list.RemoveRange(0, ConstUdpArg.WORK_FRAME_NUMS * 5 - 1);
                    br.Flush();
                    br.Close();
                    fs.Close();
                    fs = null;
                }
            }
        }

        public void EnableSaveFile() {
            UdpWaveData.SaveDataEventHandler += WriteData;
        }

        public void DisableSaveFile() {
            UdpWaveData.SaveDataEventHandler -= WriteData;
        }

        void WriteData(object sender, byte[] data) {
            var buffer = new byte[data.Length];
            Array.Copy(data, 0, buffer, 0, data.Length);
            if (list.Count < (ConstUdpArg.WORK_FRAME_NUMS * 50)) {
                list.Add(buffer);
                if (0 == list.Count % ConstUdpArg.WORK_FRAME_NUMS * 5)
                    sem.Release();
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (fs != null) fs.Dispose();
                if (br != null) br.Dispose();
                if (sem != null) sem.Dispose();
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
