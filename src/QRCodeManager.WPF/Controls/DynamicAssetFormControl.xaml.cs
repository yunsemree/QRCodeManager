using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace QRCodeManager.WPF.Controls;

public partial class DynamicAssetFormControl : UserControl
{
    public static readonly DependencyProperty FieldsProperty =
        DependencyProperty.Register(nameof(Fields), typeof(IEnumerable), typeof(DynamicAssetFormControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(DynamicAssetFormControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowRequiredHintProperty =
        DependencyProperty.Register(nameof(ShowRequiredHint), typeof(bool), typeof(DynamicAssetFormControl),
            new PropertyMetadata(true));

    public IEnumerable? Fields
    {
        get => (IEnumerable?)GetValue(FieldsProperty);
        set => SetValue(FieldsProperty, value);
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

    public DynamicAssetFormControl()
    {
        InitializeComponent();
    }
}
