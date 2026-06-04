using System;
using System.Globalization;
using System.Windows.Data;

namespace MetGlobalCompany.WPF.Converters;

/// <summary>
/// Простой конвертер для инверсии boolean значений (используется для блокировки кнопки "Сохранить", если документ Проведен).
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}