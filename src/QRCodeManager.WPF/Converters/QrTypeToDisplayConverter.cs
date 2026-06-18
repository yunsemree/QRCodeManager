using System.Globalization;
using System.Windows.Data;
using QRCodeManager.Domain.Enums;

namespace QRCodeManager.WPF.Converters;

public class QrTypeToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            QrType.Generated => "Oluşturulan",
            QrType.Imported => "İçe Aktarılan",
            QrType.Decoded => "Okunan",
            _ => value?.ToString() ?? string.Empty
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
