using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace QRCodeManager.WPF.Helpers;

public static class ThemeHelper
{
    private static readonly PaletteHelper PaletteHelper = new();

    public static void ApplyTheme(string theme)
    {
        var isDark = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase);
        ApplyMaterialTheme(isDark);
        ApplyAppBrushes(isDark);
    }

    private static void ApplyMaterialTheme(bool isDark)
    {
        var materialTheme = PaletteHelper.GetTheme();
        materialTheme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
        materialTheme.SetPrimaryColor(isDark ? Color.FromRgb(129, 140, 248) : Color.FromRgb(79, 70, 229));
        materialTheme.SetSecondaryColor(Color.FromRgb(20, 184, 166));
        PaletteHelper.SetTheme(materialTheme);
    }

    private static void ApplyAppBrushes(bool isDark)
    {
        if (System.Windows.Application.Current is null)
        {
            return;
        }

        var resources = System.Windows.Application.Current.Resources;

        if (isDark)
        {
            SetBrush(resources, "AppBackgroundBrush", Color.FromRgb(17, 24, 39));
            SetBrush(resources, "SidebarBrush", Color.FromRgb(31, 41, 55));
            SetBrush(resources, "SidebarBorderBrush", Color.FromRgb(55, 65, 81));
            SetBrush(resources, "CardSurfaceBrush", Color.FromRgb(31, 41, 55));
            SetBrush(resources, "CardBorderBrush", Color.FromRgb(55, 65, 81));
            SetBrush(resources, "NavActiveBrush", Color.FromRgb(55, 65, 81));
            SetBrush(resources, "NavHoverBrush", Color.FromRgb(45, 55, 72));
            SetBrush(resources, "MutedTextBrush", Color.FromRgb(156, 163, 175));
            SetBrush(resources, "TitleTextBrush", Color.FromRgb(249, 250, 251));
            SetBrush(resources, "PreviewPanelBrush", Color.FromRgb(24, 33, 47));
            SetBrush(resources, "TitleBarButtonHoverBrush", Color.FromRgb(55, 65, 81));
        }
        else
        {
            SetBrush(resources, "AppBackgroundBrush", Color.FromRgb(244, 245, 247));
            SetBrush(resources, "SidebarBrush", Color.FromRgb(255, 255, 255));
            SetBrush(resources, "SidebarBorderBrush", Color.FromRgb(232, 234, 237));
            SetBrush(resources, "CardSurfaceBrush", Color.FromRgb(255, 255, 255));
            SetBrush(resources, "CardBorderBrush", Color.FromRgb(232, 234, 237));
            SetBrush(resources, "NavActiveBrush", Color.FromRgb(238, 240, 244));
            SetBrush(resources, "NavHoverBrush", Color.FromRgb(246, 247, 249));
            SetBrush(resources, "MutedTextBrush", Color.FromRgb(107, 114, 128));
            SetBrush(resources, "TitleTextBrush", Color.FromRgb(17, 24, 39));
            SetBrush(resources, "PreviewPanelBrush", Color.FromRgb(250, 250, 250));
            SetBrush(resources, "TitleBarButtonHoverBrush", Color.FromRgb(229, 231, 235));
        }

        SetBrush(resources, "AccentPurpleBrush", Color.FromRgb(139, 92, 246));
        SetBrush(resources, "AccentAmberBrush", Color.FromRgb(245, 158, 11));
        SetBrush(resources, "AccentTealBrush", Color.FromRgb(20, 184, 166));
        SetBrush(resources, "AccentRoseBrush", Color.FromRgb(244, 63, 94));
        SetBrush(resources, "TitleBarCloseHoverBrush", Color.FromRgb(232, 17, 35));
    }

    private static void SetBrush(ResourceDictionary resources, string key, Color color)
    {
        if (resources[key] is SolidColorBrush brush && brush.IsFrozen)
        {
            resources[key] = CreateBrush(color);
            return;
        }

        if (resources[key] is SolidColorBrush existing)
        {
            existing.Color = color;
            return;
        }

        resources[key] = CreateBrush(color);
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
