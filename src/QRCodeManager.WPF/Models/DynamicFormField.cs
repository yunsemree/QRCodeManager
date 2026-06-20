using CommunityToolkit.Mvvm.ComponentModel;

namespace QRCodeManager.WPF.Models;

public partial class DynamicFormField : ObservableObject
{
    public DynamicFormField(string key, string label, bool isRequired, string value = "")
    {
        Key = key;
        Label = label;
        IsRequired = isRequired;
        _value = value;
        Hint = isRequired ? $"{label} *" : label;
    }

    public string Key { get; }
    public string Label { get; }
    public bool IsRequired { get; }
    public string Hint { get; }

    [ObservableProperty]
    private string _value;
}
