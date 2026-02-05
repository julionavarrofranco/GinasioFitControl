using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FitControlAdmin.Converters
{
    /// <summary>
    /// Converts true -> Collapsed, false -> Visible (inverse of BoolToVisibility)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
