using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using ArrayDisplay.UI;

namespace ArrayDisplay.net {
    /// <summary>
    ///     处理与分发网络数据
    /// </summary>
    public class Dataproc : IDisposable {
        public Dataproc() {
            TransFormFft = new FFT_TransForm();
            ArrayNums = ConstUdpArg.ARRAY_NUM; //探头个数

            FreqWaveEvent = new AutoResetEvent(false);
            WorkEnergyEvent = new AutoResetEvent(false);
            DelayBytesEvent = new AutoResetEvent(false);
            OrigBytesEvent = new AutoResetEvent(false);
            WorkBytesEvent = new AutoResetEvent(false);
            IsBavlueReaded = false;
        }

        public Thread FreqThread {
            get;
            set;
        }

        public int ArrayNums {
            get;
            set;
        }

        public void Init(ConstUdpArg.WaveType type) {
            WaveType = type;
            switch(type) {
                case ConstUdpArg.WaveType.Delay:
                    DelayInit();
                    break;
                case ConstUdpArg.WaveType.Normal:
                    NormalInit();
                    break;
                case ConstUdpArg.WaveType.Orig:
                    OrigInit();
                    break;
                 default:
                    Console.WriteLine("初始化Dataproc出错");
                    break;
            }
        }

        void OrigInit() {
            int oNums = ConstUdpArg.ORIG_DETECT_LENGTH; //时分长度
            OrigWaveBytes = new byte[oNums][];
            OrigWaveFloats = new float[oNums][];
            OrigChannnel = 0;
            OrigTimeDiv = 0;

            int oFrime = DisPlayWindow.SelectdInfo.OrigFramNums; //原始数据帧数
            Origdata = new byte[oFrime * ArrayNums];
            int olength = ConstUdpArg.ORIG_FRAME_LENGTH - 2; //每帧长度
            for (int i = 0; i < oNums; i++)
            {
                OrigWaveBytes[i] = new byte[olength * oFrime];
                OrigWaveFloats[i] = new float[olength / 2 * oFrime];

            }
            OrigThread = new Thread(ThreadOrigWaveStart) { IsBackground = true };
            OrigThread.Start();
        }

        void NormalInit() {
            TransFormFft = new FFT_TransForm();
            WorkWaveBytes = new byte[ArrayNums][];
            WorkWaveFloats = new float[ArrayNums][];
            WorkWaveBytes = new byte[ArrayNums][];
            PlayWaveBytes = new byte[ArrayNums][];

            int frameNum = ConstUdpArg.WORK_FRAME_NUMS * 4; //工作数据帧数
//            int frameNum = 2000 * ConstUdpArg.WORK_FRAME_LENGTH; //工作数据帧数
            for (int i = 0; i < ArrayNums; i++)
            {
                WorkWaveBytes[i] = new byte[frameNum * 4]; //避免数据为null
                WorkWaveFloats[i] = new float[frameNum]; //避免数据为null 
                PlayWaveBytes[i] = new byte[frameNum * 2]; //避免数据为null 
            }
            WorkWavefdatas = new float[frameNum]; //工作波形
            EnergyFloats = new float[ArrayNums];
            ListenCoefficent = (float)Math.Pow(10, 50 / 20.0F) * 100; //31622.78; //听音强度

            WorkThread = new Thread(ThreadWorkWaveStart) { IsBackground = true };
            WorkThread.Start();
            EnergyThread = new Thread(ThreadEnergyStart) { IsBackground = true };
            EnergyThread.Start();
            FreqThread = new Thread(ThreadFreqWaveStart) { IsBackground = true };
            FreqThread.Start();
        }

        void DelayInit() {
            int dchannels = ConstUdpArg.DELAY_FRAME_CHANNELS;
            int dframeLen = ConstUdpArg.DELAY_FRAME_LENGTH - 2;
            int dframeNum = ConstUdpArg.DELAY_FRAME_NUMS;
            DelayWaveBytes = new byte[dchannels][];
            DelayWaveFloats = new float[dchannels][];
            for (int i = 0; i < dchannels; i++)
            {
                DelayWaveBytes[i] = new byte[dframeLen * dframeNum];
                DelayWaveFloats[i] = new float[dframeLen * dframeNum / 2];
            }
            DelayThread = new Thread(ThreadDelayWaveStart) { IsBackground = true };
            DelayThread.Start();
            
        }

        /// <summary>
        ///     正在传输波形
        /// </summary>
        public ConstUdpArg.WaveType WaveType {
            get;
            set;
        }
        public Thread EnergyThread {
            get;
            set;
        }

        public Thread WorkThread {
            get;
            set;
        
        }

        public Thread DelayThread {
            get;

            set;

        }

        public Thread OrigThread {
            get;
            set;
        }

        /// <summary>
        ///     线程处理函数：延时数据处理
        /// </summary>
        void ThreadDelayWaveStart() {
            while(true) {
                DelayBytesEvent.WaitOne();
                var r = new byte[2];

                for(int i = 0; i < DelayWaveBytes.Length; i++) {
                    for(int j = 0; j < (DelayWaveBytes[0].Length / 2); j++) {
                        r[0] = DelayWaveBytes[i][j * 2 + 1];
                        r[1] = DelayWaveBytes[i][j * 2 + 0];
                        short a = BitConverter.ToInt16(r, 0);
                        DelayWaveFloats[i][j] = a / 8192.0f;

                        //                        OrigWave_Floats[i][j] = a / 104800.0f;
                    }
                }
                DelayGraphEventHandler(null, DelayWaveFloats);
            }
        }
        /// <summary>
        /// 线程处理函数：原始数据处理
        /// </summary>
        void ThreadOrigWaveStart() {
            while(true) {
                OrigBytesEvent.WaitOne();
                var r = new byte[2];
                for (int i = 0; i < ConstUdpArg.ORIG_DETECT_LENGTH; i++)
                {
                    for (int j = 0; j < OrigWaveBytes[0].Length / 2 - 1; j++)
                    {
                        r[0] = OrigWaveBytes[i][j * 2 + 1];
                        r[1] = OrigWaveBytes[i][j * 2];
                        short a = BitConverter.ToInt16(r, 0);

                        OrigWaveFloats[i][j] = a / 8192.0f;
                    }
                }
                if (OrigGraphEventHandler != null)
                {
                    OrigGraphEventHandler.Invoke(null, OrigWaveFloats);
                }
            }
        }

        

        /// <summary>
        ///线程处理函数：能量数据处理
        /// </summary>
        void ThreadEnergyStart() {
            while(true) {
                WorkEnergyEvent.WaitOne();
                double ftemp = 0;
                for(int i = 0; i < WorkWaveFloats.Length; i++) {
                        float f = WorkWaveFloats[i].Max();
                    ftemp = Math.Abs(f);
                    EnergyFloats[i] = (float) ftemp;
                }
                float max = EnergyFloats.Max();
                for(int i = 0; i < EnergyFloats.Length; i++) {
                    if (Math.Abs(max) > float.Epsilon) {
                        EnergyFloats[i] = EnergyFloats[i] / max;
                    }
                    else {
                        EnergyFloats[i] = 0;
                    }
                    
                }
                List<float> rlist = new List<float>();
                for(int i = 0; i < EnergyFloats.Length; i++) {
                    if ((i%32)<8) {
                        rlist.Add(EnergyFloats[i]);
                    }
                }
                
                if (EnergyArrayEventHandler != null) {
                    EnergyArrayEventHandler.Invoke(null, rlist.ToArray());
                }
            }
        }

        /// <summary>
        /// 线程函数处理：正常工作数据处理
        /// </summary>
        void ThreadWorkWaveStart() {
            while(true) {
                WorkBytesEvent.WaitOne();
                var r = new byte[4];
                for(int i = 0; i < ConstUdpArg.ARRAY_NUM; i++) {
                    for(int j = 0; j < ConstUdpArg.WORK_FRAME_NUMS; j++) {
                        r[0] = WorkWaveBytes[i][j * 4 + 3];
                        r[1] = WorkWaveBytes[i][j * 4 + 2];
                        r[2] = WorkWaveBytes[i][j * 4 + 1];
                        r[3] = WorkWaveBytes[i][j * 4];
                        int a = BitConverter.ToInt32(r, 0);
                        float tmp = a / 1048576.0f;
//                        float tmp = a / 2.0f;
                        WorkWaveFloats[i][j] = tmp;

                        //听音数据处理
                        float f = WorkWaveFloats[i][j] * ListenCoefficent;
                        short sh;
                        if (f > 32767) {
                            sh = 32767;
                        }
                        else if (f <= -32767) {
                            sh = -32767;
                        }
                        else {
                            sh = (short) f;
                        }
                        var x = BitConverter.GetBytes(sh);
                        Array.Copy(x, 0, PlayWaveBytes[i], j * 2, 2);
                    }
                }
                var offset = ConstUdpArg.offsetArray[DisPlayWindow.SelectdInfo.WorkWaveChannel-1];
                WorkWavefdatas = WorkWaveFloats[offset-1];
                WorkEnergyEvent.Set();
                FreqWaveEvent.Set();


                if (PreGraphEventHandler != null)
                {
                    PreGraphEventHandler.Invoke(null, WorkWavefdatas);
                }
                //            if (BckGraphEventHandler != null) BckGraphEventHandler.Invoke(null, WorkWaveTwo);

                if (SoundEventHandler != null)
                {
                    int channel = DisPlayWindow.SelectdInfo.WorkWaveChannel;
                    if (channel > 0)
                    {
                        SoundEventHandler.Invoke(null, PlayWaveBytes[channel]);
                    }
                }
               
            }
        }

        /// <summary>
        /// 线程处理函数：正常工作数据处理
        /// </summary>
        void ThreadFreqWaveStart() {
            while(true) {
                FreqWaveEvent.WaitOne();
                FreqWaveOne = TransFormFft.FFT(WorkWavefdatas);
                var dataPoints = NewFFT.Start(WorkWavefdatas);
//                if (FrapGraphEventHandler != null) {
//                    FrapGraphEventHandler.Invoke(null, FreqWaveOne);
//                }
                if (FrapPointGraphEventHandler != null) {
                    FrapPointGraphEventHandler(null, dataPoints);   
                }
            }
        }

//        /// <summary>
//        /// 计算初始相位（基于原始数据）
//        /// </summary>
//        /// <returns></returns>
//        double[] GetInitPhase() {
//            float[][] phaseFloats = new float[OrigWaveFloats.Length][];
//            double[][] cosDoubles = new double[phaseFloats.Length][];
//            double[][] sinDoubles = new double[phaseFloats.Length][];
//            for(int i = 0; i < phaseFloats.Length; i++) {
//                phaseFloats[i] = new float[OrigWaveFloats[i].Length];
//                cosDoubles[i] = new double[OrigWaveFloats[i].Length];
//                sinDoubles[i] = new double[OrigWaveFloats[i].Length];
//            }
//            int n = OrigWaveFloats[0].Length; //一帧数据
//            try {
//                for(int i = 0; i < phaseFloats.Length; i++) {
//                    Array.Copy(OrigWaveFloats[i], 0, phaseFloats[i], 0, n);
//                }
//            }
//            catch(Exception e) {
//                Console.WriteLine(e);
//                throw;
//            }
//                        
//            for(int i = 0; i < sinDoubles.Length; i++) {
//                for(int j = 0; j < sinDoubles[i].Length; j++) {
//                    cosDoubles[i][j] = phaseFloats[i][j] * Math.Cos(2 * Math.PI *31.25 * 1000 *j / n);   //31.25Khz
//                    sinDoubles[i][j] = phaseFloats[i][j] * Math.Sin(2 * Math.PI *31.25 * 1000 *j / n);
//                }
//            }
//            double[] phase = new double[phaseFloats.Length];
//            for(int i = 0; i < phase.Length; i++) {
//                phase[i] =(Math.PI - Math.Atan(sinDoubles[i].Average() / cosDoubles[i].Average()))*Math.PI/180;
//            }
//            return phase;
//        }

        /// <summary>
        ///     计算初始相位（工作波形）
        /// </summary>
        /// <returns></returns>
        double[] GetInitPhase() {
            var phaseFloats = new float[WorkWaveFloats.Length][];
            var cosDoubles = new double[phaseFloats.Length][];
            var sinDoubles = new double[phaseFloats.Length][];
            for(int i = 0; i < phaseFloats.Length; i++) {
                phaseFloats[i] = new float[WorkWaveFloats[i].Length];
                cosDoubles[i] = new double[WorkWaveFloats[i].Length];
                sinDoubles[i] = new double[WorkWaveFloats[i].Length];
            }
            int n = WorkWaveFloats[0].Length; //一帧数据
            try {
                for(int i = 0; i < phaseFloats.Length; i++) {
                    Array.Copy(WorkWaveFloats[i], 0, phaseFloats[i], 0, n);
                }
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }

            for(int i = 0; i < sinDoubles.Length; i++) {
                for(int j = 0; j < sinDoubles[i].Length; j++) {
                    cosDoubles[i][j] = phaseFloats[i][j] * Math.Cos(2 * Math.PI * 31.25 * 1000 * j / n); //31.25Khz
                    sinDoubles[i][j] = phaseFloats[i][j] * Math.Sin(2 * Math.PI * 31.25 * 1000 * j / n);
                }
            }
            var phase = new double[phaseFloats.Length];
            for(int i = 0; i < phase.Length; i++) {
                phase[i] = (Math.PI - Math.Atan(sinDoubles[i].Average() / cosDoubles[i].Average())) * Math.PI / 180;
            }
            return phase;
        }

        

        

        #region 变量

        /// <summary>
        ///     事件通知
        /// </summary>
        public AutoResetEvent WorkBytesEvent {
            get;
            set;
        }

        public AutoResetEvent OrigBytesEvent {
            get;
            set;
        }

        public AutoResetEvent DelayBytesEvent {
            get;
            set;
        }

        public AutoResetEvent WorkEnergyEvent {
            get;
            set;
        }

        public AutoResetEvent FreqWaveEvent {
            get;
            set;
        }

        /// <summary>
        ///     事件句柄
        /// </summary>
        public static EventHandler<byte[]> SoundEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> PreGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> BckGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> EnergyArrayEventHandler {
            get;
            set;
        }

        public static EventHandler<float[][]> OrigGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> FrapGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<Point[]> FrapPointGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[][]> DelayGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     外界数据
        /// </summary>
        public int OrigChannnel {
            get;
            set;
        }
         public int OrigTimeDiv
        {
            get;
            set;
        }
        public byte[][] DelayWaveBytes {
            get;
            set;
        }

        public byte[][] OrigWaveBytes {
            get;
            set;
        }

        public byte[][] WorkWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     内部数据
        /// </summary>
        float[] WorkWavefdatas {
            get;
            set;
        }

        float[] WorkWaveTwo {
            get;
            set;
        }

        float[] FreqWaveOne {
            get;
            set;
        }

        float[][] DelayWaveFloats {
            get;
            set;
        }

        

        FFT_TransForm TransFormFft {
            get;
            set;
        }

        byte[] Origdata {
            get;
            set;
        }

        float[][] WorkWaveFloats {
            get;
            set;
        }

        /// <summary>
        ///     能量图数据
        /// </summary>
        float[] EnergyFloats {
            get;
            set;
        }

        float[][] OrigWaveFloats {
            get;
            set;
        }

        byte[][] PlayWaveBytes {
            get;
            set;
        }

        //听音系数
        float ListenCoefficent {
            get;
            set;
        }

        public static bool IsBavlueReaded {
            get;
            set;
        }

        public int[] IsRcvChanneldata {
            get;
            set;
        }

        #endregion


        #region IDisposable

        void ReleaseUnmanagedResources() {
            // TODO release unmanaged resources here
        }
        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                if (WorkBytesEvent != null) WorkBytesEvent.Dispose();
                if (OrigBytesEvent != null) OrigBytesEvent.Dispose();
                if (DelayBytesEvent != null) DelayBytesEvent.Dispose();
                if (WorkEnergyEvent != null) WorkEnergyEvent.Dispose();
                if (FreqWaveEvent != null) FreqWaveEvent.Dispose();
                if (OrigThread != null)
                {
                    OrigThread.Abort();
                    if (OrigThread.ThreadState!=ThreadState.Aborted) {
                        Thread.Sleep(100);
                    }
                }
                if (DelayThread != null)
                {
                    DelayThread.Abort();
                }
                if (WorkThread != null)
                {
                    WorkThread.Abort();
                    if (WorkThread.ThreadState != ThreadState.Aborted)
                    {
                        Thread.Sleep(100);
                    }
                    
                }
                if (EnergyThread != null)
                {
                    EnergyThread.Abort();
                }
                if (FreqThread != null)
                {
                    FreqThread.Abort();
                }
            }
        }
        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
            
        }

        /// <inheritdoc />
        ~Dataproc() {
            Dispose(false);
        }

        #endregion
    }
}
