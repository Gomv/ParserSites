
using Avalonia;                      
using Avalonia.Data.Converters;      
using System;
using System.Globalization;

namespace ParserSites.Converters;

public class DepthToMarginConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int depth = value is int d ? d : 0;
        int step = parameter is string s && int.TryParse(s, out int custom) ? custom : 16;
        return new Thickness(depth * step, 1, 0, 1);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}