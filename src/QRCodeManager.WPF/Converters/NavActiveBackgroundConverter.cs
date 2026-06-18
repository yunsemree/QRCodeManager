using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using QRCodeManager.WPF.Models;

namespace QRCodeManager.WPF.Converters;

public class NavActiveBackgroundConverter : IMultiValueConverter
{
    public static NavActiveBackgroundConverter Instance { get; } = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not AppPage current)
        {
            return Brushes.Transparent;
        }

        AppPage targetPage = values[1] switch
        {
            AppPage page => page,
            string pageName when Enum.TryParse<AppPage>(pageName, out var parsed) => parsed,
            _ => AppPage.Generate
        };

        if (current != targetPage)
        {
            return Brushes.Transparent;
        }

        return System.Windows.Application.Current?.TryFindResource("NavActiveBrush") as Brush ?? Brushes.LightGray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
