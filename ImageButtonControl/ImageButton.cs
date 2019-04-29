using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageButtonControl
{
    /// <summary>
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    /// </summary>
    /// 

    public class ImageButton : Button
    {
        #region 属性
        /// <summary>
        /// 按钮处于正常状态下的背景图片的路径
        /// </summary>
        public string NormalBackgroundImage
        {
            get { return (string)GetValue(NormalBackgroundImageProperty); }
            set { SetValue(NormalBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 鼠标移到按钮上面，按钮的背景图片的路径
        /// </summary>
        public string MouseoverBackgroundImage
        {
            get { return (string)GetValue(MouseoverBackgroundImageProperty); }
            set { SetValue(MouseoverBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 鼠标按下按钮，按钮的背景图片的路径
        /// </summary>
        public string MousedownBackgroundImage
        {
            get { return (string)GetValue(MousedownBackgroundImageProperty); }
            set { SetValue(MousedownBackgroundImageProperty, value); }
        }

        /// <summary>
        /// 当按钮不可用时按钮的背景图片
        /// </summary>
        public string DisabledBackgroundImage
        {
            get { return (string)GetValue(DisabledBackgroundImageProperty); }
            set { SetValue(DisabledBackgroundImageProperty, value); }
        }
        #endregion

        #region 依赖属性
        /// <summary>
        /// 按钮处于正常状态下的背景图片的路径（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty NormalBackgroundImageProperty =
            DependencyProperty.Register("NormalBackgroundImage", typeof(string), typeof(ImageButton), new PropertyMetadata(null));

        /// <summary>
        /// 鼠标移到按钮上面，按钮的背景图片的路径（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MouseoverBackgroundImageProperty =
            DependencyProperty.Register("MouseoverBackgroundImage", typeof(string), typeof(ImageButton), new PropertyMetadata(null));

        /// <summary>
        /// 鼠标按下按钮，按钮的背景图片的路径（这是依赖属性）
        /// </summary>
        public static readonly DependencyProperty MousedownBackgroundImageProperty =
            DependencyProperty.Register("MousedownBackgroundImage", typeof(string), typeof(ImageButton), new PropertyMetadata(null));

        /// <summary>
        /// 当按钮不可用时按钮的背景图片（这是一个依赖属性）
        /// </summary>
        public static readonly DependencyProperty DisabledBackgroundImageProperty =
            DependencyProperty.Register("DisabledBackgroundImage", typeof(string), typeof(ImageButton), new PropertyMetadata(null));
        #endregion

        #region 构造函数
        public ImageButton(): base()            
        {
            //读取资源字典文件
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/ImageButtonControl;component/Themes/Style.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(rd);
            //获取样式
            Style = FindResource("SimpleImageButton") as Style;
        }
        #endregion
    }
}
