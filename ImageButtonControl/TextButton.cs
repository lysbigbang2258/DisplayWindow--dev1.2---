using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageButtonControl
{
    public class TextButton:Button
    {
        #region 属性
        /// <summary>
        /// 当鼠标移到按钮上时，按钮的前景色
        /// </summary>
        public Brush MouserOverForeground
        {
            get { return (Brush)GetValue(MouserOverForegroundProperty); }
            set { SetValue(MouserOverForegroundProperty, value); }
        }

        /// <summary>
        /// 鼠标移到按钮上时，按钮的背景色
        /// </summary>
        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        /// <summary>
        /// 当鼠标按下时，按钮的前景色
        /// </summary>
        public Brush MousedownForeground
        {
            get { return (Brush)GetValue(MousedownForegroundProperty); }
            set { SetValue(MousedownForegroundProperty, value); }
        }

        /// <summary>
        /// 当鼠标按下时，按钮的背景色
        /// </summary>
        public Brush MousedownBackground
        {
            get { return (Brush)GetValue(MousedownBackgroundProperty); }
            set { SetValue(MousedownBackgroundProperty, value); }
        }

        /// <summary>
        /// 当按钮不可用时，按钮的前景色
        /// </summary>
        public Brush DisabledForeground
        {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }

        /// <summary>
        /// 当按钮不可用时，按钮的背景色
        /// </summary>
        public Brush DisabledBackground
        {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }
        #endregion

        #region 依赖属性
        /// <summary>
        /// 当鼠标移到按钮上时，按钮的前景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MouserOverForegroundProperty =
            DependencyProperty.Register("MouserOverForeground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// 鼠标移到按钮上时，按钮的背景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// 当鼠标按下时，按钮的前景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MousedownForegroundProperty =
            DependencyProperty.Register("MousedownForeground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// 当鼠标按下时，按钮的背景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MousedownBackgroundProperty =
            DependencyProperty.Register("MousedownBackground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// 当按钮不可用时，按钮的前景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.Register(" DisabledForeground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// 当按钮不可用时，按钮的背景色（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(TextButton), new PropertyMetadata(Brushes.White));
        #endregion

        #region 构造函数
        public TextButton():base()
        {
            //获取资源文件信息
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/ImageButtonControl;component/Themes/Style.xaml", UriKind.Relative);
            this.Resources.MergedDictionaries.Add(rd);
            //设置样式
            this.Style = this.FindResource("SimpleTextButton") as Style;
        }
        #endregion
    }
}
