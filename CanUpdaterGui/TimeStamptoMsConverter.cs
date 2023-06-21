using System;
using System.Globalization;
using System.Windows.Data;

namespace CanUpdaterGui;

[ValueConversion(typeof(DateTime), typeof(string))]
public class TimeStamptoMsConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var x = (DateTime) value;
        return x.Millisecond.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}