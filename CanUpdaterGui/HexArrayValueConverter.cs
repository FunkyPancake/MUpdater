using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CanUpdaterGui;

[ValueConversion(typeof(byte[]), typeof(string))]
public class HexArrayValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var val = ((byte[]) value);
        var str = new StringBuilder(val.Length * 4 + 1);
        foreach (var b in val)
        {
            str.Append($"0x{b:x2} ");
        }

        return str.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}