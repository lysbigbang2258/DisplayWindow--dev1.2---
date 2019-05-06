/*------------------------------------------------------------------------------------------
  文 件 名: 数据类型定义
  简要说明: 
  主要功能: 
  特别约定: 
  原始作者: 
  发布日期: 20160712
  修改记录:
           001. （修改作者），（修改时间）
		        （修改内容）
------------------------------------------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace ArrayDisplay.DataFile {

    #region 常量定义

    public class DefineConst {
        #region 发送命令字段

        //发送给数据处理平台的帧标识和结束标识
        public const uint SEND_FRAME_HEAD = 0xAAAAAAAA;

        public const uint SEND_FRAME_TAIL = ~0xAAAAAAAA;

        //发送给数据处理平台的GPS字段标识和结束标识
        public const uint GPS_FIELD_HEAD = 0xB1B1B1B1;

        public const uint GPS_FIELD_TAIL = ~0xB1B1B1B1;

        //发送给数据处理平台的水文字段标识和结束标识
        public const uint SW_FIELD_HEAD = 0xB2B2B2B2;

        public const uint SW_FIELD_TAIL = ~0xB2B2B2B2;

        //发送给数据处理平台的波束积分字段标识和结束标识
        public const uint BI_FIELD_HEAD = 0xB3B3B3B3;

        public const uint BI_FIELD_TAIL = ~0xB3B3B3B3;

        //发送给数据处理平台的设置目标跟踪字段标识和结束标识
        public const uint STT_FIELD_HEAD = 0xB4B4B4B4;

        public const uint STT_FIELD_TAIL = ~0xB4B4B4B4;

        //发送给数据处理平台的波束频段字段标识和结束标识
        public const uint BFB_FIELD_HEAD = 0xB5B5B5B5;

        public const uint BFB_FIELD_TAIL = ~0xB5B5B5B5;

        //发送给数据处理平台的撤销目标跟踪字段标识和结束标识
        public const uint CTT_FIELD_HEAD = 0xB6B6B6B6;

        public const uint CTT_FIELD_TAIL = ~0xB6B6B6B6;

        //发送给数据处理平台的设置听音类型字段标识和结束标识
        public const uint SLO_FIELD_HEAD = 0xB7B7B7B7;

        public const uint SLO_FIELD_TAIL = ~0xB7B7B7B7;

        //发送给数据处理平台的取消听音类型字段标识和结束标识
        public const uint CL_FIELD_HEAD = 0xB8B8B8B8;

        public const uint CL_FIELD_TAIL = ~0xB8B8B8B8;

        //发送给数据处理平台的设置听音频段字段标识和结束标识
        public const uint SLB_FIELD_HEAD = 0xB9B9B9B9;

        public const uint SLB_FIELD_TAIL = ~0xB9B9B9B9;

        //发送给数据处理平台的视频录制字段标识和结束标识
        public const uint VR_FIELD_HEAD = 0xBABABABA;

        public const uint VR_FIELD_TAIL = ~0xBABABABA;

        //发送给数据处理平台的音频录制字段标识和结束标识
        public const uint AR_FIELD_HEAD = 0xBBBBBBBB;

        public const uint AR_FIELD_TAIL = ~0xBBBBBBBB;

        //发送给数据处理平台的视频截图字段标识和结束标识
        public const uint VS_FIELD_HEAD = 0xBCBCBCBC;

        public const uint VS_FIELD_TAIL = ~0xBCBCBCBC;

        //发送给数据处理平台的文本字段标识和结束标识
        public const uint TI_FIELD_HEAD = 0xBDBDBDBD;

        public const uint TI_FIELD_TAIL = ~0xBDBDBDBD;

        //发送给数据处理平台的设置波形通道编号字段标识和结束标识
        public const uint SCN_FIELD_HEAD = 0xBEBEBEBE;

        public const uint SCN_FIELD_TAIL = ~0xBEBEBEBE;

        //发送给数据处理平台的设置脉冲区字段标识和结束标识
        public const uint SPR_FIELD_HEAD = 0xBFBFBFBF;

        public const uint SPR_FIELD_TAIL = ~0xBFBFBFBF;

        //发送给数据处理平台的取消脉冲区字段标识和结束标识
        public const uint CPR_FIELD_HEAD = 0xC1C1C1C1;

        public const uint CPR_FIELD_TAIL = ~0xC1C1C1C1;

        //发送给数据处理平台的故障码字段标识和结束标识
        public const uint EC_FIELD_HEAD = 0xC2C2C2C2;

        public const uint EC_FIELD_TAIL = ~0xC2C2C2C2;

        //发送给数据处理平台的阵元状态字段标识和结束标识
        public const uint AE_FIELD_HEAD = 0xC3C3C3C3;

        public const uint AE_FIELD_TAIL = ~0xC3C3C3C3;

        //发送给数据处理平台的设置目标时域频段字段标识和结束标识
        public const uint TTDF_FIELD_HEAD = 0xC4C4C4C4;

        public const uint TTDF_FIELD_TAIL = ~0xC4C4C4C4;

        //发送给数据处理平台的阵列数据字段标识和结束标识
        public const uint ARRAY_DATA_FIELD_HEAD = 0xC5C5C5C5;

        public const uint ARRAY_DATA_FIELD_TAIL = ~0xC5C5C5C5;

        //发送给数据处理平台的阵列姿态字段和结束标识
        public const uint ARRAY_POSE_FIELD_HEAD = 0xC6C6C6C6;

        public const uint ARRAY_POSE_FIELD_TAIL = ~0xC6C6C6C6;

        //目标增强：设置目标跟踪
        public const uint ETST_FIELD_HEAD = 0xC7C7C7C7;

        public const uint ETST_FIELD_TAIL = ~0xC7C7C7C7;

        //目标增强：撤销目标跟踪
        public const uint ETCT_FIELD_HEAD = 0xC8C8C8C8;

        public const uint ETCT_FIELD_TAIL = ~0xC8C8C8C8;

        //目标距离
        public const uint TDT_FIELD_HEAD = 0xC9C9C9C9;

        public const uint TDT_FIELD_TAIL = ~0xC9C9C9C9;

        //波束类型
        public const uint BT_FIELD_HEAD = 0xCACACACA;

        public const uint BT_FIELD_TAIL = ~0xCACACACA;

        #endregion

        #region 接收数据字段

        //接收数据处理平台的帧标识和结束标识
        public const uint RCV_FRAME_HEAD = 0xDDDDDDDD;

        public const uint RCV_FRAME_TAIL = ~0xDDDDDDDD;

        //接收数据处理平台的波束形成字段标识和结束标识
        public const uint BD_FIELD_HEAD = 0xE1E1E1E1;

        public const uint BD_FIELD_TAIL = ~0xE1E1E1E1;

        //接收数据处理平台的目标信息字段标识和结束标识
        public const uint TD_FIELD_HEAD = 0xE2E2E2E2;

        public const uint TD_FIELD_TAIL = ~0xE2E2E2E2;

        //接收数据处理平台的脉冲信息字段标识和结束标识
        public const uint PD_FIELD_HEAD = 0xE3E3E3E3;

        public const uint PD_FIELD_TAIL = ~0xE3E3E3E3;

        //接收数据处理平台的脉冲发现谱字段标识和结束标识
        public const uint PDS_FIELD_HEAD = 0xE4E4E4E4;

        public const uint PDS_FIELD_TAIL = ~0xE4E4E4E4;

        //接收数据处理平台的听音数据字段标识和结束标识
        public const uint LD_FIELD_HEAD = 0xE5E5E5E5;

        public const uint LD_FIELD_TAIL = ~0xE5E5E5E5;

        //接收数据处理平台的通道能量字段标识和结束标识
        public const uint CE_FIELD_HEAD = 0xE6E6E6E6;

        public const uint CE_FIELD_TAIL = ~0xE6E6E6E6;

        //接收数据处理平台的通道时域频域字段标识和结束标识
        public const uint TFD_FIELD_HEAD = 0xE7E7E7E7;

        public const uint TFD_FIELD_TAIL = ~0xE7E7E7E7;

        //接收数据处理平台的回传系统故障码字段标识和结束标识
        public const uint FEC_FIELD_HEAD = 0xE8E8E8E8;

        public const uint FEC_FIELD_TAIL = ~0xE8E8E8E8;

        //接收数据处理平台的回传SVP字段标识和结束标识
        public const uint FSVP_FIELD_HEAD = 0xE9E9E9E9;

        public const uint FSVP_FIELD_TAIL = ~0xE9E9E9E9;

        //接收数据处理平台的回传GPS字段标识和结束标识
        public const uint FGPS_FIELD_HEAD = 0xEAEAEAEA;

        public const uint FGPS_FIELD_TAIL = ~0xEAEAEAEA;

        //接收数据处理平台的回传波束频段字段标识和结束标识
        public const uint FBFB_FIELD_HEAD = 0xEBEBEBEB;

        public const uint FBFB_FIELD_TAIL = ~0xEBEBEBEB;

        //接收数据处理平台的回传波束积分时间字段标识和结束标识
        public const uint FBFI_FIELD_HEAD = 0xECECECEC;

        public const uint FBFI_FIELD_TAIL = ~0xECECECEC;

        //接收数据处理平台的回传听音频段字段标识和结束标识
        public const uint FLB_FIELD_HEAD = 0xEDEDEDED;

        public const uint FLB_FIELD_TAIL = ~0xEDEDEDED;

        //接收数据处理平台的脉冲时频谱字段标识和结束标识
        public const uint PTFS_FIELD_HEAD = 0xEEEEEEEE;

        public const uint PTFS_FIELD_TAIL = ~0xEEEEEEEE;

        //人工脉冲信息字段
        public const uint MPD_FIELD_HEAD = 0xEFEFEFEF;

        public const uint MPD_FIELD_TAIL = ~0xEFEFEFEF;

        //目标增强：波束形成
        public const uint ETBD_FIELD_HEAD = 0xF1F1F1F1;

        public const uint ETBD_FIELD_TAIL = ~0xF1F1F1F1;

        //目标增强：目标信息
        public const uint ETTM_FIELD_HEAD = 0xF2F2F2F2;

        public const uint ETTM_FIELD_TAIL = ~0xF2F2F2F2;

        //目标增强：有价值目标
        public const uint ETWT_FIELD_HEAD = 0xF3F3F3F3;

        public const uint ETWT_FIELD_TAIL = ~0xF3F3F3F3;

        #endregion

        #region 宏定义

        

        //波束图    
        public const int BEAM_DATA_NUM = 720; //波束形成图的数据个数

        public const int BEAM_ANGLE_NUM = 361;

        //目标信息
        public const int MAX_TCP_LENGHT = 16569; //目标连续谱

        public const int MAX_TLP_LENGHT = 16569; //目标线谱
        public const int MAX_TWF_LENGHT = 31250; //目标波形

        public const int MAX_DEMON_LENGHT = 1050;

        //脉发现谱
        public const int NUM_TSEG_PDT = 50; //一个处理周期内(1s)进行时频分析的时间段数，这个参数对应0.02s的时间分辨率；

        public const int NUM_FRQA_PDT = 2000;

        //听音数据大小
        public const int SND_SAMPLE_RATE = 31250; //声音数据的采样率。

        //通道状态
        public const int MAX_CH_TW_LENGHT = 31250; //通道时域波形

        public const int MAX_CH_FW_LENGHT = 16384; //通道频率波形

        //水文
        public const int SW_DATA_NUM = 1000; //水文数据个数  

        //阵列数据接收
        public static uint arrayNum = 512;
        public static uint sampleFrequency = 50000;
        public static uint tcpFrameLength = 106200000;

        public static uint socketReceiveBufferSize = 106200000;

        //阵列数据发送
        public static uint arrayDataBufferSize = 106200000;

        public static uint arrayDataSocketBufferSize = 32768;

        //命令数据发送
        public static uint cmdSocketBufferSize = 8192;

        //处理结果接收
        public static uint resultSocketBufferSize = 8192;

        //波速形成
        public static int beamDataLength = 720;

        //频谱特征
        public static int tcpDataLength = 16569; //连续谱

        public static float tcpMaxFrequency = 4000;
        public static float tcpMinFrequency = 50;

        public static int tlpDataLength = 16569; //线谱
        public static float tlpMaxFrequency = 4000;
        public static float tlpMinFrequency = 50;

        public static int ttwDataLength = 31250; //目标时域波形

        public static int demonDataLength = 1050; //DEMON谱
        public static int demonMaxFreq = 50;

        public static int demonMinFreq = 4000;

        //阵元通道
        public static int timeWaveDataLength = 31250;
        public static int freqWaveDataLength = 16384;
        public static float waveFreqPrecision = 31250 / 32768.0f;
        

        //脉冲特征
        public static float timeBegin = 0;

        public static float frequencyBegin = 30;
        public static float timePrecision = 0.02f;
        public static float frequencyPrecision = 6.1035f;
        public static int timeDotNum = 50;
        public static int frequencyDotNum = 1962;
        public static int timeTotalLength = 1800;
        public static int maxFreqDotNum = 2000;

        //听音
        public static int soundSampleFreqency = 31250;

        //阵元数
        public const int ARRAY_NUM = 256; //阵元数
        //Buffer设置
       

        #endregion
    }

    #endregion

    #region 枚举类型

    public enum TargetNum {
        T1 = 1,
        T2 = 2
    }

    public enum AllDataUpdateFlag {
        FlagGpsData = 0,
        FlagSwData,
        FlagBeamIntegerate,
        FlagTargetTracing1,
        FlagTargetTracing2,
        FlagTargetTracing3,
        FlagTargetTracing4,
        FlagTargetTracing5,
        FlagTargetTracing6,
        FlagBeamFrequencyBand,
        FlagCancleTargetTarcing1,
        FlagCancleTargetTarcing2,
        FlagCancleTargetTarcing3,
        FlagCancleTargetTarcing4,
        FlagCancleTargetTarcing5,
        FlagCancleTargetTarcing6,
        FlagListenOption,
        FlagCancelListen,
        FlagListenBand,
        FlagVideoRecord,
        FlagAudioRecord,
        FlagVideoScreenShot,
        FlagTextInput,
        FlagWavformChannel,
        FlagPulseRange,
        FlagCancelPulseRange,
        FlagErrCode,
        FlagArrayElement,
        FlagTargetTimeDomainFreqLevel1,
        FlagTargetTimeDomainFreqLevel2,
        FlagArrayPose,
        FlagEnhanceTargetTracing1,
        FlagEnhanceTargetTracing2,
        FlagEnhanceTargetTracing3,
        FlagEnhanceTargetTracing4,
        FlagEnhanceTargetTracing5,
        FlagEnhanceTargetTracing6,
        FlagEnhanceTargetCancelTracing1,
        FlagEnhanceTargetCancelTracing2,
        FlagEnhanceTargetCancelTracing3,
        FlagEnhanceTargetCancelTracing4,
        FlagEnhanceTargetCancelTracing5,
        FlagEnhanceTargetCancelTracing6,
        FlagTargetDistance,
        FlagBeamType,
        Num
    }

    public enum ListenType {
        DisableListen = 0,
        TargetListen = 1,
        AngleListen = 2,
        ArrayListen = 3
    }

    #endregion

    #region 数据保存

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StGpsSaveData {
        public long ltime;
        public long lGPS_time;
        public float fGPS_Latitude;
        public float fGPS_Longitude;
        public float fGPS_Course;
        public float fGPS_Speed;
        public float fArrayLength;
        public float fArrayDepth;
        public int flag; //标识N、S、W、E半球
    }

    #endregion

    #region 发送数据处理平台字段

    //GPS字段
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StGpsData {
        public uint uGPS_FieldHead;
        public uint uGPS_FieldLength;
        public long lGPS_time;
        public float fGPS_Course;
        public float fGPS_Speed;
        public float fGPS_Longitude;
        public float fGPS_Latitude;
        public int iGPS_Reserve;
        public uint uGPS_FieldTail;
    }

    //水文数据字段 
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StSwData {
        public uint uSW_FieldHead;
        public uint uSW_FieldLength;
        public unsafe fixed float faSW_Depth [DefineConst.SW_DATA_NUM];
        public unsafe fixed float faSW_Speed [DefineConst.SW_DATA_NUM];
        public int iSW_Reserve;
        public uint uSW_FieldTail;
    }

    //波束积分 Beam Integerate
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StBeamIntegerate {
        public uint uBI_FieldHead;
        public uint uBI_FieldLength;
        public uint uBI_IntegValue;
        public uint uBI_FieldTail;
    }

    //设置目标跟踪 Set Target Tracing
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StTargetTracing {
        public uint uSTT_FieldHead;
        public uint uSTT_FieldLength;
        public uint uSTT_TargetNumber;
        public float fSTT_TargetAngle;
        public unsafe fixed int iaSTT_Reserve [3];
        public uint uSTT_FieldTail;
    }

    //波束频段 Beam Frenquency Band
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StBeamFrequencyBand {
        public uint uBFB_FieldHead;
        public uint uBFB_FieldLength;
        public uint uBFB_Band;
        public uint uBFB_FieldTail;
    }

    //撤销目标跟踪 Cancel Target Tracing
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StCancleTargetTarcing {
        public uint uCTT_FieldHead;
        public uint uCTT_FieldLength;
        public uint uCTT_TargetNumber;
        public uint uCTT_FieldTail;
    }

    //设置听音类型 Set Listening Options
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StListenOption {
        public uint uSLO_FieldHead;
        public uint uSLO_FieldLength;
        public uint uSLO_ListeningType;
        public uint uSLO_TargetNumber;
        public uint uSLO_ChannelNumber;
        public float fSLO_ListeningAngle;
        public int iSLO_Reserve;
        public uint uSLO_FieldTail;
    }

    //取消听音 Cancel Listening
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StCancelListen {
        public uint uCL_FieldHead;
        public uint uCL_FieldLength;
        public uint uCL_ListeningType;
        public uint uCL_FieldTail;
    }

    //设置听音频段 Set Listening Band
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StListenBand {
        public uint uSLB_FieldHead;
        public uint uSLB_FieldLength;
        public uint uSLB_ListeningBand;
        public uint uSLB_FieldTail;
    }

    //设置波形通道编号 Set Wavform Channel Number
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public unsafe struct StWavformChannel {
        public uint uSCN_FieldHead;
        public uint uSCN_FieldLength;
        public fixed uint uSCN_ChannelNumber [4];
        public int iSCN_Reserve;
        public uint uSCN_FieldTail;
    }

    //设置脉冲区 Set Pulse Range
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StPulseRange {
        public uint uSPR_FieldHead;
        public uint uSPR_FieldLength;
        public uint uSPR_TimeStart;
        public uint uSPR_TimeEnd;
        public uint uSPR_FreqStart;
        public uint uSPR_FreqEnd;
        public int iSPR_Reserve;
        public uint uSPR_FieldTail;
    }

    //取消脉冲区 Cancel Pulse Range
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StCancelPulseRange {
        public uint uCPR_FieldHead;
        public uint uCPR_FieldLength;
        public uint uCPR_TimeStart;
        public uint uCPR_TimeEnd;
        public uint uCPR_FreqStart;
        public uint uCPR_FreqEnd;
        public int iCPR_Reserve;
        public uint uCPR_FieldTail;
    }

    //故障码 Error Code
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StErrCode {
        public uint uEC_FieldHead;
        public uint uEC_FieldLength;
        public unsafe fixed uint uaEC_ErrorCode [100];
        public int iEC_Reserve;
        public uint uEC_FieldTail;
    }

    //阵元状态 Array State
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StArrayElement {
        public uint uAE_FieldHead;
        public uint uAE_FieldLength;
        public unsafe fixed uint uaAE_ElementState [512];
        public int iAE_Reserve;
        public uint uAE_FieldTail;
    }

    //设置目标时域频段 Set Target Time Domain Frequency
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StTargetTimeDomainFreqLevel {
        public uint uTTDF_FieldHead;
        public uint uTTDF_FieldLength;
        public int iTargetNum;
        public int iFrequencyLevel;
        public uint uTTDF_FieldTail;
    }

    //阵列姿态字段
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StArrayPose {
        public uint uArrayPose_FieldHead; //0XC6C6C6C6
        public uint uArrayPose_FieldLength;
        public float iArrayLen;
        public float iArrayDepth;
        public uint uArrayPose_FieldTail; //~0XC6C6C6C6
    }

    //目标增强：设置目标跟踪 Set Target Tracing
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StEnhanceTargetTracing {
        public uint uSTT_FieldHead;
        public uint uSTT_FieldLength;
        public uint uSTT_TargetNumber;
        public float fSTT_TargetAngle;
        public unsafe fixed int iaSTT_Reserve [3];
        public uint uSTT_FieldTail;
    }

    //目标增强：撤销目标跟踪 Cancel Target Tracing
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StEnhanceTargetCancelTarcing {
        public uint uCTT_FieldHead;
        public uint uCTT_FieldLength;
        public uint uCTT_TargetNumber;
        public uint uCTT_FieldTail;
    }

    //目标距离
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StTargetDistance {
        public uint uTDT_FieldHead;
        public uint uTDT_FieldLength;
        public uint uTDT_Type;
        public unsafe fixed float fTDT_Data [6];
        public uint uTDT_FieldTail;
    }

    //波束类型CBF/MVDR
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StBeamType {
        public uint uBT_FieldHead;
        public uint uBT_FieldLength;
        public uint uBT_Type;
        public uint uBT_FieldTail;
    }

    //发送给数据处理平台的数据结构
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct SendCmd {
        public uint uSFrameHead;
        public uint uSFrameLength;
        public long lSTime_t;
        public int iSFieldNum;

        public StGpsData GPSData;
        public StSwData SWData;
        public StBeamIntegerate BeamIntegerate;
        public StTargetTracing TargetTracing1;
        public StTargetTracing TargetTracing2;
        public StTargetTracing TargetTracing3;
        public StTargetTracing TargetTracing4;
        public StTargetTracing TargetTracing5;
        public StTargetTracing TargetTracing6;
        public StBeamFrequencyBand BeamFrequencyBand;
        public StCancleTargetTarcing CancleTargetTarcing1;
        public StCancleTargetTarcing CancleTargetTarcing2;
        public StCancleTargetTarcing CancleTargetTarcing3;
        public StCancleTargetTarcing CancleTargetTarcing4;
        public StCancleTargetTarcing CancleTargetTarcing5;
        public StCancleTargetTarcing CancleTargetTarcing6;
        public StListenOption ListenOption;
        public StCancelListen CancelListen;
        public StListenBand ListenBand;
        public StWavformChannel WavformChannel;
        public StPulseRange PulseRange;
        public StCancelPulseRange CancelPulseRange;
        public StErrCode ErrCode;
        public StArrayElement ArrayElement;
        public StTargetTimeDomainFreqLevel TargetTimeDomainFreqLevel1;
        public StTargetTimeDomainFreqLevel TargetTimeDomainFreqLevel2;
        public StArrayPose ArrayPose;
        public StEnhanceTargetTracing EnhanceTargetTracing1;
        public StEnhanceTargetTracing EnhanceTargetTracing2;
        public StEnhanceTargetTracing EnhanceTargetTracing3;
        public StEnhanceTargetTracing EnhanceTargetTracing4;
        public StEnhanceTargetTracing EnhanceTargetTracing5;
        public StEnhanceTargetTracing EnhanceTargetTracing6;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing1;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing2;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing3;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing4;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing5;
        public StEnhanceTargetCancelTarcing EnhanceTargetCancelTarcing6;
        public StTargetDistance TargetDistance;
        public StBeamType BeamType;
        public uint uSFrameChecksum;
        public uint uSFrameTail;
    }

    #endregion

    #region 接收数据处理平台字段

    //波束形成 Beam Data
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StBeamData {
        public uint uBD_FieldHead;
        public uint uBD_FieldLength;
        public unsafe fixed double faBD_BeamData [DefineConst.BEAM_DATA_NUM]; //double
        public int iBD_Reserve;
        public uint uBD_FieldTail;
    }

    //目标信息 Target Data
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StTargetData {
        public uint uTD_FieldHead;
        public uint uTD_FieldLength;
        public uint uTD_TargetNumber;
        public uint uTD_TargetState;
        public float fTD_TargetAngle;
        public float fTD_ShipAngle;
        public float fTD_Intensity;
        public float fTD_ZhouPin; //轴频
        public unsafe fixed float faTD_ContSpec [DefineConst.MAX_TCP_LENGHT];
        public unsafe fixed float faTD_LineSpec [DefineConst.MAX_TLP_LENGHT];
        public int iLinNum;
        public unsafe fixed float falinFrq [100];
        public unsafe fixed float faLinInt [100];
        public unsafe fixed float faTD_DemonSpec [DefineConst.MAX_DEMON_LENGHT]; //db
        public unsafe fixed float faTD_WaveData [DefineConst.MAX_TWF_LENGHT];
        public uint uTD_FieldTail;
    }

    //脉冲信息 Pulse Data
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StPulseData {
        public uint uPD_FieldHead;
        public uint uPD_FieldLength;
        public float uPD_PulseNumber;
        public long uPD_TimeBase; //第一帧时间
        public float uPD_TimeOffset; //算法计算后的偏移时间
        public float uPD_TimeDuration; //时间长
        public float fPD_PulseAngle;
        public float fPD_PulseIntensity;
        public float fPD_Type; //脉冲类型
        public float fPD_FreqStart;
        public float fPD_FreqWidth;
        public float fPD_Period;
        public uint uPD_FieldTail;
    }

    //脉冲时频谱 Pulse TimeFrequency Spectrum
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StPulseTimeFrequencySpectrum {
        public uint uPTFS_FieldHead;
        public uint uPTFS_FieldLength;
        // public unsafe fixed float daPTFS_TimeFreqSpec [DefineConst.NUM_FRQA_PDT * DefineConst.NUM_TSEG_PDT];  

        public unsafe fixed float daPTFS_TimeFreqAngle [DefineConst.NUM_FRQA_PDT * DefineConst.NUM_TSEG_PDT];
        public unsafe fixed float iPTFS_TimeRange [2];
        public unsafe fixed float fPTFS_FreqRange [2];
        public uint uPTFS_FieldTail;
    }

    //脉冲发现谱 Pulse Discover Spectrum
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StPulseDiscoverSpectrum {
        public uint uPDS_FieldHead;
        public uint uPDS_FieldLength;
        public unsafe fixed float faPDS_Data [DefineConst.NUM_FRQA_PDT * DefineConst.NUM_TSEG_PDT];
        public unsafe fixed float faPDS_Angle [DefineConst.NUM_FRQA_PDT * DefineConst.NUM_TSEG_PDT];
        public float fTimBgn; //起始时间 
        public float fFrqBgn; //起始频率 50 for31.25k;30 for 50K
        public float fTimSpc; //时间间隔 0.02 ,50个点
        public float fFrqSpc; //频率间隔
        public int iTimNum; //时间点数
        public int iFrqNum; //频率点数
        public int iPDS_Reserve;
        public uint uPDS_FieldTail;
    }

    //听音数据 Listening Data
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StListenData {
        public uint uLD_FieldHead;
        public uint uLD_FieldLength;
        public uint uLD_ListeningType;
        public uint uLD_TargetNumber;
        public uint uLD_ChannelNumber;
        public float fLD_ListeningAngle;
        public unsafe fixed float faLD_ListeningData [DefineConst.SND_SAMPLE_RATE];
        public uint uLD_FieldTail;
    }

    //通道能量 Channel Energy
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StChannelEnergy {
        public uint uCE_FieldHead;
        public uint uCE_FieldLength;
        public unsafe fixed float fCE_ChEnData [DefineConst.ARRAY_NUM];
        public int iCE_Reserve;
        public uint uCE_FieldTail;
    }

    //通道时域频域波形 Channel Time Frequency Data
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StChannelTimeFrequencyData {
        public uint uTFD_FieldHead;
        public uint uTFD_FieldLength;
        public uint uTFD_ChannelNumber;
        public unsafe fixed float fTFD_TimeData [DefineConst.MAX_CH_TW_LENGHT];
        public unsafe fixed float fTFD_FreqData [DefineConst.MAX_CH_FW_LENGHT];
        public uint uTFD_FieldTail;
    }

    //回传系统故障码 Feedback Error Code
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StFbSystemErrorCode {
        public uint uFEC_FieldHead;
        public uint uFEC_FieldLength;
        public unsafe fixed uint uaFEC_ErrorCode [100];
        public int iFEC_Reserve;
        public uint uFEC_FieldTail;
    }

    /// <summary>
    ///     目标增强字段
    /// </summary>
    //目标增强：波束形成
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StEnhanceTargetBeamData {
        public uint uBD_FieldHead;
        public uint uBD_FieldLength;
        public unsafe fixed double faBD_BeamData [DefineConst.BEAM_DATA_NUM]; //double
        public int iBD_Reserve;
        public uint uBD_FieldTail;
    }

    //目标增强：有价值目标
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StEnhanceTargetWorthData {
        public uint uBD_FieldHead;
        public uint uBD_FieldLength;
        public unsafe fixed double faBD_BeamData [DefineConst.BEAM_DATA_NUM]; //double
        public int iBD_Reserve;
        public uint uBD_FieldTail;
    }

    //目标增强：目标信息
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
    public struct StEnhanceTargetData {
        public uint uTD_FieldHead;
        public uint uTD_FieldLength;
        public uint uTD_TargetNumber;
        public uint uTD_TargetState;
        public float fTD_TargetAngle;
        public float fTD_ShipAngle;
        public float fTD_Intensity;
        public uint uTD_FieldTail;
    }

    #endregion

    #region 不安全内存操作

    public class ByteAndStructConvert {
        public static byte[] StructToBytes(object structobj, int size) {
            var bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structobj, structPtr, false);
            Marshal.Copy(structPtr, bytes, 0, size);
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }

        public static object ByteToStruct(byte[] bytes, Type type) {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length) return null;
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object obj = new object();
            obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }
    }

    public class ByteAndObjectCoppyTo {
        //int
        public static int[] ByteToInt(byte[] bytes) {
            int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                var managedArray = new int[bytes.Length / 4];
                Marshal.Copy(ptr, managedArray, 0, bytes.Length / 4);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        public static byte[] IntToByte(int[] ints) {
            int size = ints.Length * Marshal.SizeOf(ints[0]);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(ints, 0, ptr, ints.Length);
                var managedArray = new byte[size];
                Marshal.Copy(ptr, managedArray, 0, size);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        //float
        public static float[] ByteToFloat(byte[] bytes) {
            int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                var managedArray = new float[bytes.Length / 4];
                Marshal.Copy(ptr, managedArray, 0, bytes.Length / 4);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        public static byte[] FloatToByte(float[] floats) {
            int size = floats.Length * Marshal.SizeOf(floats[0]);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(floats, 0, ptr, floats.Length);
                var managedArray = new byte[size];
                Marshal.Copy(ptr, managedArray, 0, size);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        //double
        public static double[] ByteToDouble(byte[] bytes) {
            int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                var managedArray = new double[bytes.Length / 8];
                Marshal.Copy(ptr, managedArray, 0, bytes.Length / 8);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        public static byte[] DoubleToByte(double[] doubles) {
            int size = doubles.Length * Marshal.SizeOf(doubles[0]);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(doubles, 0, ptr, doubles.Length);
                var managedArray = new byte[size];
                Marshal.Copy(ptr, managedArray, 0, size);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        public static byte[] ShortToByte(short[] shorts) {
            int size = shorts.Length * Marshal.SizeOf(shorts[0]);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try {
                Marshal.Copy(shorts, 0, ptr, shorts.Length);
                var managedArray = new byte[size];
                Marshal.Copy(ptr, managedArray, 0, size);
                return managedArray;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }
    }

    #endregion

    #region long和datetime之间转换

    public class DateTime2Long {
        public static long ConvertDateTime2Long(DateTime dt) {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = dt.Subtract(dtStart);
            long span = toNow.Ticks / 10000000;
            return span;
        }

        public static DateTime ConvertLong2DateTime(long d) {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long ltime = d * 10000000;
            TimeSpan toNow = new TimeSpan(ltime);
            DateTime dtResult = dtStart.Add(toNow);
            return dtResult;
        }
    }

    #endregion

    #region 事件参数

    //采样数据
    public class SampleDataEventArgs : EventArgs {
        public byte[] ArrayData { set; get; }

        public byte[] GpsData { set; get; }
    }

    //波束图
    public class BeamgraphEventArgs : EventArgs {
        public float[] Msg { get; set; }
    }

    //目标标识
    public class BeamMarkEventArgs : EventArgs {
        public float para1;
        public float para2;
        public int target;

        public BeamMarkEventArgs(int target, float para1, float para2) {
            this.target = target;
            this.para1 = para1;
            this.para2 = para2;
        }
    }

    //波束历程图
    public class BeamgraphHistoryEventArgs : EventArgs {
        public float[] Msg { get; set; }
    }

    //目标信息
    public class TargetInfoEventArgs : EventArgs {
        public float[] Msg { get; set; }
    }

    //线谱
    public class LineSpectrumEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Msg { get; set; }
    }

    public class LineSpecDataEventArgs : EventArgs {
        public int Num { get; set; }

        public int Lines { get; set; }

        public float MaxValue { get; set; }

        public float MinValue { get; set; }

        public float[] FreqRange { get; set; }

        public float[] Data { get; set; }
    }

    //连续谱
    public class ContinueSpectrumEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Data { get; set; }
    }

    //目标时域波形
    public class TargetTimeWaveEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Msg { get; set; }
    }

    //DEMON谱
    public class DemonEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Data { get; set; }
    }

    public class DemonDataEventArgs : EventArgs {
        public int Num { get; set; }

        public int Lines { get; set; }

        public float MaxValue { get; set; }

        public float MinValue { get; set; }

        public float[] FreqRange { get; set; }

        public float[] Data { get; set; }
    }

    //清空线谱
    public class ClearLineSpectrumEventArgs : EventArgs {
        public bool Flag { set; get; }

        public int Num { set; get; }
    }

    //清空DEMON谱
    public class ClearDemonEventArgs : EventArgs {
        public bool Flag { set; get; }

        public int Num { set; get; }
    }

    //脉冲信息
    public class PulseInfoEventArgs : EventArgs {
        public int Flag { set; get; }

        public float[] Msg { set; get; }
    }

    public class PulseOnScreenEventArgs : EventArgs {
        public string[] Data { set; get; }
    }

    //脉冲时频谱
    public class PtfEventArgs : EventArgs {
        public float[] TimeRange { get; set; }

        public float[] FrequencyRange { get; set; }

        public float[,] Data { get; set; }

        public float[,] DataDirection { get; set; }
    }

    //脉冲时频谱图形数据显示事件
    public class PtfPixelsEventArgs : EventArgs {
        public float[] TimeRange { set; get; }

        public float[] FreqRange { set; get; }

        public float[,] SrcData { set; get; }

        public byte[] Pixels { set; get; }
    }

    //脉冲发现谱
    public class PulseDiscoverySpectrumEventArgs : EventArgs {
        public float TimeBegin //起始时间 
        { get; set; }

        public float FreqBegin //起始频率 50 for31.25k;30 for 50K
        { get; set; }

        public float TimePrecision //时间间隔 0.02 ,50个点
        { get; set; }

        public float FreqPrecision //频率间隔
        { get; set; }

        public int TimeNum //时间点数
        { get; set; }

        public int FreqNum //频率点数 
        { get; set; }

        public float[] Data { get; set; }

        public float[] DataDirection { get; set; }
    }

    public class PdsPixelsEventArgs : EventArgs {
        public float Max { get; set; }

        public float Min { get; set; }

        public uint Count { get; set; }

        public float[] Data { get; set; }
    }

    //播放声音
    public class PlaySoundEventArgs : EventArgs {
        public byte[] Data { get; set; }
    }

    //存储声音
    public class StoreSoundEventArgs : EventArgs {
        public byte[] Data { get; set; }
    }

    //通道能量
    public class ArrayEnergyEventArgs : EventArgs {
        public float[] Data { get; set; }
    }

    //通道时域频域
    public class ArrayWaveEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] TimeData { get; set; }

        public float[] FreqData { get; set; }
    }

    public class ArrayTimeWaveEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Data { get; set; }
    }

    public class ArrayFrequencyWaveEventArgs : EventArgs {
        public int Num { get; set; }

        public float[] Data { get; set; }
    }

    //故障码
    public class ErrCodeEventArgs : EventArgs {
        public uint[] ErrCode { get; set; }
    }

    //GPSData
    public class GpsDataEventArgs : EventArgs {
        public DateTime Time { set; get; }

        public string StrLatitude { set; get; }

        public string StrLatitudeFlag { set; get; }

        public string StrLongitude { set; get; }

        public string StrLongitudeFlag { set; get; }

        public string StrHeading { set; get; }

        public string StrSpeed { set; get; }
    }

    //回放
    public class PlayStateEventArgs : EventArgs {
        public int Flag { set; get; }

        public int FileFirstLength { set; get; }

        public int FileOtherLength { set; get; }

        public int CurrentLength { set; get; }
    }

    //传感器
    public class SensorStreamEventArgs : EventArgs {
        public byte[] Depth { set; get; }

        public byte[] Pose { set; get; }
    }

    public class SensorValueEventArgs : EventArgs {
        public float[] Depth { set; get; }

        public float[] Pose { set; get; }
    }

    //目标增强：波束形成
    public class EnhanceTargetBeamEventArgs : EventArgs {
        public float[] Data { set; get; }
    }

    //目标增强：有价值目标
    public class EnhanceTargetWorthEventArgs : EventArgs {
        public float[] Data { set; get; }
    }

    //目标增强：目标信息
    public class EnhanceTargetDataEventArgs : EventArgs {
        public float[] Data { set; get; }
    }

    //低频目标波束历程
    public class BarBeamHistoryDataEventArgs : EventArgs {
        public byte[] Pixels { set; get; }

        public long Count { set; get; }
    }

    //有价值目标历程
    public class WorthHistoryDataEventArgs : EventArgs {
        public byte[] Pixels { set; get; }

        public long Count { set; get; }
    }

    #endregion
}
