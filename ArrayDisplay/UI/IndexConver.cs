// 201812284:26 PM

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArrayDisplay.UI {
    [ValueConversion(typeof(float), typeof(ListViewItem))]
    public class IndexConver : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = value as ListViewItem;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

            var index = listView.ItemContainerGenerator.IndexFromContainer(item);
            string indexStr = string.Format("通道{0}时分{1}B",1+ index % 8, 1+ index / 8);
            return listView != null && item != null ? indexStr : null;  
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    } 
}

