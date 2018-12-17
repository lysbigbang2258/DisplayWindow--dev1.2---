using System.ComponentModel;
using System.Runtime.CompilerServices;
using ArrayDisplay.Annotations;

namespace ArrayDisplay.net {
    public class SystemInfo : INotifyPropertyChanged {
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
        public int ChannelDelayNums {
            get {
                return channelDelayNums;
            }
            set {
                channelDelayNums = value;
                OnPropertyChanged();
            }
        }
        //通道延迟值
        public int ChannelDelayTime {
            get {
                return channelDelayTime;
            }
            set {
                channelDelayTime = value;
                OnPropertyChanged();
            }
        }

        public int DacLenth {
            get
            {
                return  daclen;
            }
            set
            {
                daclen = value;
                OnPropertyChanged();
            }
        }

        public int DacChannel {
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


        #endregion

        #region Field

        int adcNum = -1;
        string adcOffset = string.Empty;
        int channelDelayNums = 1;
        int channelDelayTime = 0;
        string mcId = string.Empty;
        string mcMac = string.Empty;
        string mcType = string.Empty;
        int pulseDelay = -1;
        int pulsePeriod = -1;
        int pulseWidth = -1;
        int daclen = 3000;
        int dacChannel = 2;
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
                                                                                                                               
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
