using System.Globalization;
using System.Windows.Data;

namespace WindowInspector.App.Converters;

public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = string.Empty;
    public string FalseValue { get; set; } = string.Empty;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && boolValue ? TrueValue : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 