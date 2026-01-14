using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace FitControlAdmin.Converters
{
    public class EnumNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            
            var enumName = value.ToString() ?? string.Empty;
            // Adiciona espaço antes de letras maiúsculas (exceto a primeira)
            return Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

