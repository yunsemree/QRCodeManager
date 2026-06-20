using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.WPF.Models;

namespace QRCodeManager.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private AppPage _currentPage = AppPage.Generate;

    public bool IsSidebarVisible => CurrentPage is AppPage.Generate or AppPage.Read or AppPage.History or AppPage.Settings;

    public int MainContentColumn => IsSidebarVisible ? 1 : 0;

    public int MainContentColumnSpan => IsSidebarVisible ? 1 : 2;

    public string? CurrentUserDisplay =>
        _authService.IsAuthenticated ? _authService.CurrentUser?.DisplayName : null;

    public string PageTitle => CurrentPage switch
    {
        AppPage.Login => "Giriş",
        AppPage.Register => "Kayıt Ol",
        AppPage.Generate => "QR Oluştur",
        AppPage.Read => "QR Oku",
        AppPage.History => "Geçmiş",
        AppPage.Settings => "Ayarlar",
        _ => "QR Manager"
    };

    public string PageSubtitle => CurrentPage switch
    {
        AppPage.Login => "Hesabınıza giriş yapın veya atlayarak devam edin",
        AppPage.Register => "Yeni hesap oluşturun veya atlayarak devam edin",
        AppPage.Generate => "Eser bilgilerinizi girin ve QR kodunuzu oluşturun",
        AppPage.Read => "QR görüntüsü yükleyin ve eser bilgilerini okuyun",
        AppPage.History => "Oluşturduğunuz ve okuduğunuz QR kayıtları",
        AppPage.Settings => "Uygulama tercihlerinizi yönetin",
        _ => string.Empty
    };

    public LoginViewModel LoginViewModel { get; }
    public RegisterViewModel RegisterViewModel { get; }
    public GenerateQrViewModel GenerateQrViewModel { get; }
    public ReadQrViewModel ReadQrViewModel { get; }
    public HistoryViewModel HistoryViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public object CurrentViewModel => CurrentPage switch
    {
        AppPage.Login => LoginViewModel,
        AppPage.Register => RegisterViewModel,
        AppPage.Generate => GenerateQrViewModel,
        AppPage.Read => ReadQrViewModel,
        AppPage.History => HistoryViewModel,
        AppPage.Settings => SettingsViewModel,
        _ => GenerateQrViewModel
    };

    private readonly IAuthService _authService;

    public MainViewModel(
        IAuthService authService,
        LoginViewModel loginViewModel,
        RegisterViewModel registerViewModel,
        GenerateQrViewModel generateQrViewModel,
        ReadQrViewModel readQrViewModel,
        HistoryViewModel historyViewModel,
        SettingsViewModel settingsViewModel,
        Services.INavigationMessenger navigationMessenger)
    {
        _authService = authService;
        LoginViewModel = loginViewModel;
        RegisterViewModel = registerViewModel;
        GenerateQrViewModel = generateQrViewModel;
        ReadQrViewModel = readQrViewModel;
        HistoryViewModel = historyViewModel;
        SettingsViewModel = settingsViewModel;

        LoginViewModel.AuthSucceeded += EnterApplication;
        LoginViewModel.AuthSkipped += EnterApplication;
        LoginViewModel.RequestRegister += () => CurrentPage = AppPage.Register;

        RegisterViewModel.AuthSucceeded += EnterApplication;
        RegisterViewModel.AuthSkipped += EnterApplication;
        RegisterViewModel.RequestLogin += () => CurrentPage = AppPage.Login;

        SettingsViewModel.FieldDefinitionsChanged += () => GenerateQrViewModel.ReloadFields();

        navigationMessenger.NavigateWithPayload += OnNavigateWithPayload;
    }

    public void InitializeNavigation()
    {
        if (!_authService.IsAuthenticated && !_authService.IsAuthSkipped)
        {
            CurrentPage = AppPage.Login;
        }
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        if (Enum.TryParse<AppPage>(page, out var target))
        {
            CurrentPage = target;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        CurrentPage = AppPage.Login;
    }

    private void EnterApplication() => CurrentPage = AppPage.Generate;

    partial void OnCurrentPageChanged(AppPage value)
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(CurrentViewModel));
        OnPropertyChanged(nameof(IsSidebarVisible));
        OnPropertyChanged(nameof(MainContentColumn));
        OnPropertyChanged(nameof(MainContentColumnSpan));
        OnPropertyChanged(nameof(CurrentUserDisplay));

        if (value == AppPage.History)
        {
            _ = HistoryViewModel.LoadHistoryAsync();
        }
    }

    private void OnNavigateWithPayload(string action, string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        if (action is "regenerate" or "open")
        {
            CurrentPage = AppPage.Generate;
            GenerateQrViewModel.LoadJson(payload);
        }
    }
}
