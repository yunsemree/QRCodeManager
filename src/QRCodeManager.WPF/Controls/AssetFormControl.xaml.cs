using System.Windows;
using System.Windows.Controls;

namespace QRCodeManager.WPF.Controls;

public partial class AssetFormControl : UserControl
{
    public static readonly DependencyProperty UrunProperty =
        DependencyProperty.Register(nameof(Urun), typeof(string), typeof(AssetFormControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MateryalProperty =
        DependencyProperty.Register(nameof(Materyal), typeof(string), typeof(AssetFormControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SahibiProperty =
        DependencyProperty.Register(nameof(Sahibi), typeof(string), typeof(AssetFormControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty KonumuProperty =
        DependencyProperty.Register(nameof(Konumu), typeof(string), typeof(AssetFormControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SeriNoProperty =
        DependencyProperty.Register(nameof(SeriNo), typeof(string), typeof(AssetFormControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(AssetFormControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowRequiredHintProperty =
        DependencyProperty.Register(nameof(ShowRequiredHint), typeof(bool), typeof(AssetFormControl),
            new PropertyMetadata(true));

    public string Urun
    {
        get => (string)GetValue(UrunProperty);
        set => SetValue(UrunProperty, value);
    }

    public string Materyal
    {
        get => (string)GetValue(MateryalProperty);
        set => SetValue(MateryalProperty, value);
    }

    public string Sahibi
    {
        get => (string)GetValue(SahibiProperty);
        set => SetValue(SahibiProperty, value);
    }

    public string Konumu
    {
        get => (string)GetValue(KonumuProperty);
        set => SetValue(KonumuProperty, value);
    }

    public string SeriNo
    {
        get => (string)GetValue(SeriNoProperty);
        set => SetValue(SeriNoProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool ShowRequiredHint
    {
        get => (bool)GetValue(ShowRequiredHintProperty);
        set => SetValue(ShowRequiredHintProperty, value);
    }

    public AssetFormControl()
    {
        InitializeComponent();
    }
}
