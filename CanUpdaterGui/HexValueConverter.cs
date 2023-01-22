using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CanUpdaterGui;

[ValueConversion(typeof(int), typeof(string))]
public class HexValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return $"0x{((uint)value):x}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}