using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Stegano.UI.Converters;
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null; // visible when no image
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}