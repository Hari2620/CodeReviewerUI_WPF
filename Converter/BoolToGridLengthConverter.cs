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
    public class BoolToGridLengthConverter : IValueConverter
    {
        // ConverterParameter is the default width, e.g., 250
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool collapsed = (bool)value;
            if (collapsed)
                return new GridLength(0);
            else
                return new GridLength(System.Convert.ToDouble(parameter));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
