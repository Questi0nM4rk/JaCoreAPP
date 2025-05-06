using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace JaCoreUI.ValueConverters;

public class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCompleted && isCompleted)
        {
            return 0.5; // Dim completed items
        }
        // Return full opacity for false or null (not started or in progress)
        return 1.0; 
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 