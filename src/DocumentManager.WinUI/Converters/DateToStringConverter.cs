using Microsoft.UI.Xaml.Data;

namespace DocumentManager.WinUI.Converters;

public sealed class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is DateTime date
            ? date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
            : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

