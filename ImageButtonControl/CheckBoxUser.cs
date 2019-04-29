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
   public class CheckBoxUser : CheckBox
    {
        public CheckBoxUser():base()
        {
            //读取资源字典文件
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/ImageButtonControl;component/Themes/Style.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(rd);
            //获取样式
            Style = FindResource("SimpleCheckBox") as Style;
        }
    }
}
