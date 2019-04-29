// 201812284:30 PM
namespace ArrayDisplay.Net {
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using ArrayDisplay.Annotations;

    /// <summary>
    /// The system info.
    /// </summary>
    public sealed class SystemInfo : INotifyPropertyChanged {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInfo"/> class.
        /// </summary>
        public SystemInfo() {
            mcId = string.Empty;
            mcMac = string.Empty;
            mcType = string.Empty;
            pulseDelay = -1;
            pulsePeriod = -1;
            pulseWidth = -1;
            adcNum = 1;

            adcOffset = string.Empty;
            delayChannel = 1;
            delayTime = 1;

            daclen = 3000;
            dacChannel = 2;
            origFrams = 200;
            origChannel = 1;
            origTdiv = 1;
        }

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Field

        /// <summary>
        /// The mc id.
        /// </summary>
        string mcId;

        /// <summary>
        /// The mc mac.hun
        /// </summary>
        string mcMac;

        /// <summary>
        /// The mc type.
        /// </summary>
        string mcType;

        /// <summary>
        /// The pulse delay.
        /// </summary>
        int pulseDelay;

        /// <summary>
        /// The pulse period.
        /// </summary>
        int pulsePeriod;

        /// <summary>
        /// The pulse width.
        /// </summary>
        int pulseWidth;

        /// <summary>
        /// The adc num.
        /// </summary>
        int adcNum;

        /// <summary>
        /// The adc offset.
        /// </summary>
        string adcOffset;

        /// <summary>
        /// The delay channel.
        /// </summary>
        int delayChannel;

        /// <summary>
        /// The delay time.
        /// </summary>
        int delayTime;

        /// <summary>
        /// The daclen.
        /// </summary>
        int daclen;

        /// <summary>
        /// The dac channel.
        /// </summary>
        int dacChannel;

        /// <summary>
        /// The orig frams.
        /// </summary>
        int origFrams;

        /// <summary>
        /// The orig channel.
        /// </summary>
        int origChannel;

        /// <summary>
        /// The orig tdiv.
        /// </summary>
        int origTdiv;

        /// <summary>
        /// The work channel.
        /// </summary>
        int workChannel;

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the mc type.
        /// 设备类型
        /// </summary>
        public string McType {
            get => mcType;

            set {
                if (value == mcType) {
                    return;
                }
                mcType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the mc id.
        /// 设备ID
        /// </summary>
        public string McId {
            get => mcId;

            set {
                if (value == mcId) {
                    return;
                }
                mcId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the mc mac.
        /// 设备MAC
        /// </summary>
        public string McMac {
            get => mcMac;

            set {
                if (value != mcMac) {
                    mcMac = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the adc offset.
        /// adc偏移
        /// </summary>
        public string AdcOffset {
            get => adcOffset;

            set {
                if (value != adcOffset) {
                    adcOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the adc num.
        /// adc偏移.adcID
        /// </summary>
        public int AdcNum {
            get => adcNum;

            set {
                if (value != adcNum) {
                    adcNum = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse period.
        /// 脉冲周期
        /// </summary>
        public int PulsePeriod {
            get => pulsePeriod;

            set {
                if (value != pulsePeriod) {
                    pulsePeriod = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse delay.
        /// 脉冲延时
        /// </summary>
        public int PulseDelay {
            get => pulseDelay;

            set {
                if (value != pulseDelay) {
                    pulseDelay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse width.
        /// 脉冲宽度
        /// </summary>
        public int PulseWidth {
            get => pulseWidth;

            set {
                if (value != pulseWidth) {
                    pulseWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the delay channel.
        /// 延时通道
        /// </summary>
        public int DelayChannel {
            get => delayChannel;

            set {
                delayChannel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the delay time.
        /// 延时时间量
        /// </summary>
        public int DelayTime {
            get => delayTime;

            set {
                delayTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the work channel.
        /// 工作通道
        /// </summary>
        public int WorkChannel {
            get => workChannel;

            set {
                workChannel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the dac lenth.
        /// DAC长度
        /// </summary>
        public int DacLenth {
            get => daclen;

            set {
                daclen = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the dac channel.
        /// Dac通道
        /// </summary>
        public int DacChannel {
            get => dacChannel;

            set {
                dacChannel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the orig fram nums.
        /// 原始数据帧数
        /// </summary>
        public int OrigFramNums {
            get => origFrams;

            set {
                origFrams = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the orig channel.
        /// // 原始通道
        /// </summary>
        public int OrigChannel {
            get => origChannel;

            set => origChannel = value;
        }

        /// <summary>
        /// Gets or sets the orig tdiv.
        /// 原始时分
        /// </summary>
        public int OrigTdiv {
            get => origTdiv;

            set => origTdiv = value;
        }
        #endregion
    }
}
