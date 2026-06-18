using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using QRCodeManager.WPF.Models;

namespace QRCodeManager.WPF.Converters;

public class PageToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppPage current || parameter is not string target)
        {
            return Visibility.Collapsed;
        }

        return Enum.TryParse<AppPage>(target, out var page) && current == page
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class PageToNavForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppPage current || parameter is not string target)
        {
            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
        }

        var isActive = Enum.TryParse<AppPage>(target, out var page) && current == page;
        return new SolidColorBrush(isActive ? Color.FromRgb(79, 70, 229) : Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class HistoryIndexToAccentConverter : IValueConverter
{
    private static readonly string[] AccentKeys =
    [
        "AccentPurpleBrush",
        "AccentAmberBrush",
        "AccentTealBrush",
        "AccentRoseBrush"
    ];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int index)
        {
            return System.Windows.Application.Current?.TryFindResource("AccentPurpleBrush");
        }

        var key = AccentKeys[Math.Abs(index) % AccentKeys.Length];
        return System.Windows.Application.Current?.TryFindResource(key) ?? Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
