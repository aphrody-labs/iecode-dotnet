using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace IESE.Converter;

public class MaxLinkLevelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int linkLevel)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        return linkLevel < 10 ? 2 : 3;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
