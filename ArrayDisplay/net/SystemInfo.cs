// 201903282:33 PM

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ArrayDisplay.Annotations;

namespace ArrayDisplay.net {
    public class SystemInfo : INotifyPropertyChanged
    {
        #region Field

        string mcId = string.Empty;
        string mcMac = string.Empty;
        string mcType = string.Empty;
        int pulseDelay = -1;
        int pulsePeriod = -1;
        int pulseWidth = -1;
        int adcNum = 1;

        string adcOffset = string.Empty;
        int delayChannel = 1;
        int delayTime;
        
        int daclen = 3000;
        int dacChannel = 2;
        int origFrams = 20;
        int origChannel = 1;
        int origTdiv = 1;

        int workChannel = 1;
        #endregion

        #region Property
        /// <summary>设备类型</summary>
        public string McType {
            get {
                return mcType;
            }
            set {
                if (value != mcType) {
                    mcType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>设备ID</summary>
        public string McId {
            get {
                return mcId;
            }
            set {
                if (value != mcId) {
                    mcId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>设备MAC</summary>
        public string McMac {
            get {
                return mcMac;
            }
            set {
                if (value != mcMac) {
                    mcMac = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>adc偏移</summary>
        public string AdcOffset {
            get {
                return adcOffset;
            }
            set {
                if (value != adcOffset) {
                    adcOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>adc偏移.adcID</summary>
        public int AdcNum {
            get {
                return adcNum;
            }
            set {
                if (value != adcNum) {
                    adcNum = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>脉冲周期</summary>
        public int PulsePeriod {
            get {
                return pulsePeriod;
            }
            set {
                if (value != pulsePeriod) {
                    pulsePeriod = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>脉冲延时</summary>
        public int PulseDelay {
            get {
                return pulseDelay;
            }
            set {
                if (value != pulseDelay) {
                    pulseDelay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>脉冲宽度</summary>
        public int PulseWidth {
            get {
                return pulseWidth;
            }
            set {
                if (value != pulseWidth) {
                    pulseWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        //延迟通道号
        public int DelayChannel {
            get {
                return delayChannel;
            }
            set {
                delayChannel = value;
                OnPropertyChanged();
            }
        }

        //通道延迟值
        public int DelayTime {
            get {
                return delayTime;
            }
            set {
                delayTime = value;
                OnPropertyChanged();
            }
        }

        //工作波形
        public int WorkChannel
        {
            get
            {
                return workChannel;
            }
            set
            {
                workChannel = value;
                OnPropertyChanged();
            }
        }

        
        //Dac长度
        public int DacLenth
        {
            get
            {
                return daclen;
            }
            set
            {
                daclen = value;
                OnPropertyChanged();
            }
        }
        //Dac通道
        public int DacChannel
        {
            get
            {
                return dacChannel;
            }
            set
            {
                dacChannel = value;
                OnPropertyChanged();
            }
        }
        //原始数据帧数
        public int OrigFramNums
        {
            get
            {
                return origFrams;
            }
            set
            {
                origFrams = value;
                OnPropertyChanged();
            }
        }
        //原始通道
        public int OrigChannel {
            get {
                return origChannel;
            }
            set {
                origChannel = value;
            }
        }
        //原始时分
        public int OrigTdiv {
            get {
                return origTdiv;
            }
            set {
                origTdiv = value;
            }
        }

        #endregion

        #region Method
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        

        #endregion
        
    }
}
