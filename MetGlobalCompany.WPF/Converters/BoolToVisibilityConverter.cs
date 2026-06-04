using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetGlobalCompany.WPF.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool bValue = value is bool b && b;

        if (parameter != null && parameter.ToString()!.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            bValue = !bValue;
        }

        return bValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}