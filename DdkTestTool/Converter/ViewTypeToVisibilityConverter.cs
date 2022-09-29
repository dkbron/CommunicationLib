using DdkTestTool.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DdkTestTool.Converter
{
    public class ViewTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is SocketManager.ViewType result)
            {
                if (result == SocketManager.ViewType.TcpServer)
                    return Visibility.Visible;
            }
            
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewTypeToVisibilityConverter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is SocketManager.ViewType result)
            {
                if (result == SocketManager.ViewType.TcpServerClient || result == SocketManager.ViewType.TcpClient || result == SocketManager.ViewType.UdpClient)
                    return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewTypeToVisibilityConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is SocketManager.ViewType result)
            {
                if (result != SocketManager.ViewType.UdpClient || result != SocketManager.ViewType.UdpServer)
                    return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
