using System;
using System.Globalization;
using System.IO;
using System.Net;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace IESE.Converter;

public class BitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        try
        {
            return new Bitmap(AssetLoader.Open(new Uri(path)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
