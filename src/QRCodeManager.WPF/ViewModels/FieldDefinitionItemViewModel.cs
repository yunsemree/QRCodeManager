using CommunityToolkit.Mvvm.ComponentModel;

namespace QRCodeManager.WPF.ViewModels;

public partial class FieldDefinitionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private int _sortOrder;
}
