using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace MetGlobalCompany.WPF.Converters;

public class EnumDescriptionConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        FieldInfo? fi = value.GetType().GetField(value.ToString() ?? string.Empty);
        if (fi != null)
        {
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString() ?? string.Empty;
        }
        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return System.Windows.Data.Binding.DoNothing;
    }
}