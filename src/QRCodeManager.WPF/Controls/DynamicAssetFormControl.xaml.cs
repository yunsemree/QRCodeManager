using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QRCodeManager.WPF.Controls;

public partial class DynamicAssetFormControl : UserControl
{
    public static readonly DependencyProperty FieldsProperty =
        DependencyProperty.Register(nameof(Fields), typeof(IEnumerable), typeof(DynamicAssetFormControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowRequiredHintProperty =
        DependencyProperty.Register(nameof(ShowRequiredHint), typeof(bool), typeof(DynamicAssetFormControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AddFieldCommandProperty =
        DependencyProperty.Register(nameof(AddFieldCommand), typeof(ICommand), typeof(DynamicAssetFormControl));

    public static readonly DependencyProperty RemoveFieldCommandProperty =
        DependencyProperty.Register(nameof(RemoveFieldCommand), typeof(ICommand), typeof(DynamicAssetFormControl));

    public IEnumerable? Fields
    {
        get => (IEnumerable?)GetValue(FieldsProperty);
        set => SetValue(FieldsProperty, value);
    }

    public bool ShowRequiredHint
    {
        get => (bool)GetValue(ShowRequiredHintProperty);
        set => SetValue(ShowRequiredHintProperty, value);
    }

    public ICommand? AddFieldCommand
    {
        get => (ICommand?)GetValue(AddFieldCommandProperty);
        set => SetValue(AddFieldCommandProperty, value);
    }

    public ICommand? RemoveFieldCommand
    {
        get => (ICommand?)GetValue(RemoveFieldCommandProperty);
        set => SetValue(RemoveFieldCommandProperty, value);
    }

    public DynamicAssetFormControl()
    {
        InitializeComponent();
    }
}
