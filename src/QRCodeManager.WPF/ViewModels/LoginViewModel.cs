using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? AuthSucceeded;
    public event Action? RequestRegister;
    public event Action? AuthSkipped;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        var result = await _authService.LoginAsync(Email, Password);
        StatusMessage = result.Message;

        if (result.Success)
        {
            AuthSucceeded?.Invoke();
        }
    }

    [RelayCommand]
    private void GoToRegister() => RequestRegister?.Invoke();

    [RelayCommand]
    private async Task SkipAsync()
    {
        await _authService.SkipAsync();
        StatusMessage = "Giriş atlandı.";
        AuthSkipped?.Invoke();
    }
}
