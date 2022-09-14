using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DdkTestTool.Converter
{
    /// <summary>
    /// A converter that takes in a boolean and returns a thickness of 2 if true, useful for applying 
    /// border radius on a true value
    /// </summary>
    public class BooleanToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (bool)value ? 2 : 0;
            else
                return (bool)value ? 0 : 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
