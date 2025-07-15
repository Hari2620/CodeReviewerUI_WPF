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
    // Converts a bool (IsSidebarOpen) to a GridLength for sidebar width.
    public class BoolToSidebarWidthConverter : IValueConverter
    {
        public double OpenWidth { get; set; } = 250; // default sidebar width

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOpen = value is bool b && b;
            return isOpen ? OpenWidth : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
                return width > 0;
            return false;
        }
    }
}
