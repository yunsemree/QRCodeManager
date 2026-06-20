using CommunityToolkit.Mvvm.ComponentModel;

namespace QRCodeManager.WPF.Models;

public partial class DynamicFormField : ObservableObject
{
    public DynamicFormField(string key, string label, bool isRequired, string value = "")
    {
        _key = key;
        _label = label;
        _isRequired = isRequired;
        _value = value;
        UpdateHint();
    }

    [ObservableProperty]
    private string _key;

    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private string _value;

    [ObservableProperty]
    private string _hint = string.Empty;

    partial void OnLabelChanged(string value) => UpdateHint();

    partial void OnIsRequiredChanged(bool value) => UpdateHint();

    private void UpdateHint() =>
        Hint = IsRequired ? $"{Label} *" : Label;
}
