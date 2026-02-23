using System;
using System.Globalization;
using System.Windows.Data;

namespace FitControlAdmin.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && TimeSpan.TryParse(str, out var result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }
    }
}
