using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.WPF.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? AuthSucceeded;
    public event Action? RequestLogin;
    public event Action? AuthSkipped;

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            StatusMessage = "Şifreler eşleşmiyor.";
            return;
        }

        var result = await _authService.RegisterAsync(DisplayName, Email, Password);
        StatusMessage = result.Message;

        if (result.Success)
        {
            AuthSucceeded?.Invoke();
        }
    }

    [RelayCommand]
    private void GoToLogin() => RequestLogin?.Invoke();

    [RelayCommand]
    private async Task SkipAsync()
    {
        await _authService.SkipAsync();
        StatusMessage = "Kayıt atlandı.";
        AuthSkipped?.Invoke();
    }
}
