using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace QRCodeManager.WPF.Controls;

public partial class CustomTitleBar : UserControl
{
    public CustomTitleBar()
    {
        InitializeComponent();
    }

    private Window? HostWindow => Window.GetWindow(this);

    private void OnDragAreaMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var window = HostWindow;
        if (window is null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximize(window);
            return;
        }

        window.DragMove();
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) =>
        HostWindow?.SetCurrentValue(Window.WindowStateProperty, WindowState.Minimized);

    private void OnMaximizeClick(object sender, RoutedEventArgs e)
    {
        var window = HostWindow;
        if (window is not null)
        {
            ToggleMaximize(window);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) =>
        HostWindow?.Close();

    private void ToggleMaximize(Window window)
    {
        if (window.WindowState == WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
            MaximizeIcon.Kind = PackIconKind.WindowMaximize;
        }
        else
        {
            window.WindowState = WindowState.Maximized;
            MaximizeIcon.Kind = PackIconKind.WindowRestore;
        }
    }
}
