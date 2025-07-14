using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeReviewerApp.Converter
{
    public class BoolToAppliedBrushConverter : IValueConverter
    {
        public Brush AppliedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(215, 248, 220));
        public Brush DefaultBrush { get; set; } = Brushes.White;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isApplied = value is bool b && b;
            return isApplied ? AppliedBrush : DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
