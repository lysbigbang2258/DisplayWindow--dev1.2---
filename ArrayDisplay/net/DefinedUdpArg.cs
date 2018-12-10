using System;
using System.Net;

namespace ArrayDisplay.net {
    public class ConstUdpArg {
        #region IP与端口field

        static ConstUdpArg() {
            Src_OrigWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8974);
            SrcNorm_WaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8975);
            Dst_WorkDatIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8976);
            Src_DelayWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8973);
            Dst_ComMsgIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8972);
            Dst_ComDatIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8973);
            Src_ComDatIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8972);
            Src_ComMsgIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8971);
            SwicthToStateWindow = swicthToStateWindow;
            SwicthToDeleyWindow = swicthToDeleyWindow;
            SwicthToNormalWindow = swicthToNormalWindow;
            SwicthToOriginalWindow = swicthToOriginalWindow;
        }

        #endregion

        #region IP Property

        /// <summary>
        ///     src端发送与回传
        /// </summary>
        public static IPEndPoint Src_ComMsgIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端数据通信
        /// </summary>
        public static IPEndPoint Src_ComDatIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端命令发送与回传
        /// </summary>
        public static IPEndPoint Dst_ComMsgIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端数据通信
        /// </summary>
        public static IPEndPoint Dst_ComDatIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端延时波形数据
        /// </summary>
        public static IPEndPoint Src_DelayWaveIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端波形数据
        /// </summary>
        public static IPEndPoint Dst_WorkDatIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端延时波形数据
        /// </summary>
        public static IPEndPoint SrcNorm_WaveIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端原始波形数据
        /// </summary>
        public static IPEndPoint Src_OrigWaveIp {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        public static byte[] SwicthToStateWindow {
            get {
                return swicthToStateWindow;
            }
            private set { }
        }

        public static byte[] SwicthToOriginalWindow {
            get {
                return swicthToOriginalWindow;
            }
            private set { }
        }

        public static byte[] SwicthToDeleyWindow {
            get {
                return swicthToDeleyWindow;
            }
            private set { }
        }

        public static byte[] SwicthToNormalWindow {
            get {
                return swicthToNormalWindow;
            }
            private set { }
        }

        #endregion

        #region 指令常量Property

        public static byte[] Device_Type {
            get {
                return device_Type;
            }
        }

        public static byte[] Adc_Err {
            get {
                return adc_Err;
            }
        }

        public static byte[] Device_Id {
            get {
                return device_Id;
            }
        }

        public static byte[] Device_Mac {
            get {
                return device_Mac;
            }
        }

        /// <summary>读指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Read {
            get {
                return pulse_Period_Read;
            }
        }

        /// <summary>读指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Read {
            get {
                return pulse_Delay_Read;
            }
        }

        /// <summary>读指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Read {
            get {
                return pulse_Width_Read;
            }
        }

        /// <summary>写指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Write {
            get {
                return pulse_Period_Write;
            }
        }

        /// <summary>写指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Write {
            get {
                return pulse_Delay_Write;
            }
        }

        /// <summary>写指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Write {
            get {
                return pulse_Width_Write;
            }
        }

        /// <summary>存指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Save {
            get {
                return pulse_Period_Save;
            }
        }

        /// <summary>存指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Save {
            get {
                return pulse_Delay_Save;
            }
        }

        /// <summary>存指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Save {
            get {
                return pulse_Width_Save;
            }
        }

        /// <summary>读指令:延时通道</summary>
        public static byte[] DelayChannel_Read {
            get {
                return delayChannel_Read;
            }
        }

        /// <summary>写指令:延时通道</summary>
        public static byte[] DelayChannel_Write {
            get {
                return delayChannel_Write;
            }
        }

        /// <summary>写指令:延时通道</summary>
        public static byte[] DelayChannel_Save {
            get {
                return delayChannel_Save;
            }
        }

        #endregion

        #region 指令变量Method

        /// <summary>读指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetRead(int idcNum) {
            byte[] adcOffset = {0, 0, 1, 0, 0, 82};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>写指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetWrite(int idcNum) {
            byte[] adcOffset = {1, 0, 1, 0, 0, 82};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>存指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetSave(int idcNum) {
            byte[] adcOffset = {1, 0, 1, 2, 0, 146};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>读指令:延时通道</summary>
        public static byte[] GetDelayTimeReadCommand(int idcNum) {
            var channel = new byte[DelayChannel_Read.Length];
            Array.Copy(DelayChannel_Read, 0, channel, 0, DelayChannel_Read.Length);
            channel.SetValue((byte) (channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }

        /// <summary>写指令:延时通道</summary>
        public static byte[] GetDelayTimeWriteCommand(int idcNum) {
            var channel = new byte[DelayChannel_Write.Length];
            Array.Copy(DelayChannel_Write, 0, channel, 0, DelayChannel_Write.Length);
            channel.SetValue((byte) (channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }

        /// <summary>存指令:ADC偏移</summary>
        public static byte[] GetDelayTimeSaveCommand(int idcNum) {
            var channel = new byte[DelayChannel_Write.Length];
            Array.Copy(DelayChannel_Save, 0, channel, 0, DelayChannel_Write.Length);
            channel.SetValue((byte) (channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }

        #endregion

        #region 结构体

        public struct UdpData {
            public IPEndPoint Ip {
                get;
                set;
            }

            public byte[] DataBytes {
                get;
                set;
            }
        }

        public enum WaveType {
            Normal = 0,
            Orig = 1,
            Delay = 2
        }

        #endregion

        #region 指令field

        /// <summary>
        ///     获取状态指令
        /// </summary>
        static readonly byte[] device_Type = {0, 0, 8, 5, 0, 0};
        static readonly byte[] adc_Err = {0, 0, 3, 0, 0, 61};
        static readonly byte[] device_Id = {0, 0, 8, 5, 0, 8};
        static readonly byte[] device_Mac = {0, 0, 6, 4, 0, 26};

        /// <summary>
        ///     切换窗口指令
        /// </summary>
        static readonly byte[] swicthToStateWindow = {1, 0, 1, 0, 0, 0, 0};

        static readonly byte[] swicthToOriginalWindow = {1, 0, 1, 0, 0, 0, 2};
        static readonly byte[] swicthToDeleyWindow = {1, 0, 1, 0, 0, 0, 1};
        static readonly byte[] swicthToNormalWindow = {1, 0, 1, 0, 0, 0, 4};

        /// <summary>
        ///     读指令
        /// </summary>
        static readonly byte[] pulse_Period_Read = {0, 0, 2, 0, 0, 1};

        static readonly byte[] pulse_Delay_Read = {0, 0, 2, 0, 0, 5};
        static readonly byte[] pulse_Width_Read = {0, 0, 2, 0, 0, 3};

        /// <summary>
        ///     写指令
        /// </summary>
        static readonly byte[] pulse_Period_Write = {1, 0, 2, 0, 0, 1};

        static readonly byte[] pulse_Delay_Write = {1, 0, 2, 0, 0, 5};
        static readonly byte[] pulse_Width_Write = {1, 0, 2, 0, 0, 3};

        /// <summary>
        ///     存指令
        /// </summary>
        static readonly byte[] pulse_Period_Save = {1, 0, 1, 2, 0, 65};

        static readonly byte[] pulse_Delay_Save = {1, 0, 1, 2, 0, 69};
        static readonly byte[] pulse_Width_Save = {1, 0, 1, 2, 0, 67};

        /// <summary>
        ///     删除指令
        /// </summary>
        static readonly byte[] delayChannel_Read = {0, 0, 2, 0, 0, 10};

        static readonly byte[] delayChannel_Write = {1, 0, 2, 0, 0, 10};
        static readonly byte[] delayChannel_Save = {1, 0, 2, 0, 0, 74};


        #endregion

        #region 常量定义

        //阵元数
        public const int ARRAY_NUM = 256; //阵元数
        //Buffer设置
//        public const int WORK_FRAME_NUMS = 31250; //正常工作波形同时显示帧数
        public const int WORK_FRAME_NUMS = 1024 * 16 * 2; //正常工作波形同时显示帧数
        public const int WORK_FRAME_LENGTH = 1024; // 正常工作波形帧长
        //能量图像素长度
        public const int MAX_ENERGY_PIXELS_LENGTH = 70;
        public const int MIN_ENERGY_PIXELS_LENGTH = 0;
        public const int DEAFULT_ENERGY_PIXELS_LENGTH = 30;

        public const int ORIG_FRAME_NUMS = 20; //原始工作波形同时显示帧数
        public const int ORIG_FRAME_LENGTH = 1282; //原始工作波形帧长
        public const int ORIG_DETECT_LENGTH = 64;  
        public const int ORIG_TIME_NUMS = 8;
        public const int ORIG_CHANNEL_NUMS = 8;

        public const int DELAY_FRAME_CHANNELS = 8;
        public const int DELAY_FRAME_NUMS = 10;
        public const int DELAY_FRAME_LENGTH = 402;

        #endregion
    }
}
