using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CodeReviewerApp.Converter
{
    public class BoolToSidebarGridLengthConverter : IValueConverter
    {
        public double OpenWidth { get; set; } = 250;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen ? new GridLength(OpenWidth) : new GridLength(0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength length)
                return length.Value > 0;
            return false;
        }
    }

}
