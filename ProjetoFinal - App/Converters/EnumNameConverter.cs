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
            
            // Casos especiais de exibição
            if (enumName == "MBWay") return "MBWay";
            if (enumName == "Bracos") return "Braços";
            // Função: exibir Receção e PT sem espaço
            if (enumName == "Rececao") return "Receção";
            if (enumName == "PT") return "PT";
            if (enumName == "Admin") return "Admin";

            // Dias da semana
            if (enumName == "Segunda") return "Segunda";
            if (enumName == "Terca") return "Terça";
            if (enumName == "Quarta") return "Quarta";
            if (enumName == "Quinta") return "Quinta";
            if (enumName == "Sexta") return "Sexta";
            if (enumName == "Sabado") return "Sábado";
            if (enumName == "Domingo") return "Domingo";
            
            // Adicionar espaço antes de maiúsculas
            return Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

