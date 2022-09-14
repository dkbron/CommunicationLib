using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DdkTestTool.Converter
{
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageSource = (string)value;
            if (string.IsNullOrEmpty(imageSource))
                return new BitmapImage(null);

            return new BitmapImage(new Uri($"pack://application:,,,/{imageSource}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
