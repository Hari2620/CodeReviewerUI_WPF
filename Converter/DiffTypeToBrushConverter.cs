using CodeReviewerApp.Models;
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
    public class DiffTypeToBrushConverter : IValueConverter
    {
        // You can move these colors to a ResourceDictionary later for full theming.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DiffType.Added => new SolidColorBrush(Color.FromRgb(230, 255, 237)),      // #E6FFED
                DiffType.Removed => new SolidColorBrush(Color.FromRgb(255, 235, 235)),    // #FFEBEB
                DiffType.Collapsed => new SolidColorBrush(Color.FromRgb(240, 240, 240)),  // #F0F0F0 (chunk header)
                DiffType.Imaginary => new SolidColorBrush(Color.FromRgb(245, 245, 245)),  // #F5F5F5 (very light for empty alignment)
                DiffType.Unchanged => new SolidColorBrush(Colors.White),                  // plain white
                _ => new SolidColorBrush(Colors.White)
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
