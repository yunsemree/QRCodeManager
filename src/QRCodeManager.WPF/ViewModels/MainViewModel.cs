using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCodeManager.WPF.Models;

namespace QRCodeManager.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private AppPage _currentPage = AppPage.Generate;

    public string PageTitle => CurrentPage switch
    {
        AppPage.Generate => "QR Oluştur",
        AppPage.Read => "QR Oku",
        AppPage.History => "Geçmiş",
        AppPage.Settings => "Ayarlar",
        _ => "QR Code Manager"
    };

    public string PageSubtitle => CurrentPage switch
    {
        AppPage.Generate => "Eser bilgilerinizi girin ve QR kodunuzu oluşturun",
        AppPage.Read => "QR görüntüsü yükleyin ve eser bilgilerini okuyun",
        AppPage.History => "Oluşturduğunuz ve okuduğunuz QR kayıtları",
        AppPage.Settings => "Uygulama tercihlerinizi yönetin",
        _ => string.Empty
    };

    public GenerateQrViewModel GenerateQrViewModel { get; }
    public ReadQrViewModel ReadQrViewModel { get; }
    public HistoryViewModel HistoryViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    public object CurrentViewModel => CurrentPage switch
    {
        AppPage.Generate => GenerateQrViewModel,
        AppPage.Read => ReadQrViewModel,
        AppPage.History => HistoryViewModel,
        AppPage.Settings => SettingsViewModel,
        _ => GenerateQrViewModel
    };

    public MainViewModel(
        GenerateQrViewModel generateQrViewModel,
        ReadQrViewModel readQrViewModel,
        HistoryViewModel historyViewModel,
        SettingsViewModel settingsViewModel,
        Services.INavigationMessenger navigationMessenger)
    {
        GenerateQrViewModel = generateQrViewModel;
        ReadQrViewModel = readQrViewModel;
        HistoryViewModel = historyViewModel;
        SettingsViewModel = settingsViewModel;

        navigationMessenger.NavigateWithPayload += OnNavigateWithPayload;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        if (Enum.TryParse<AppPage>(page, out var target))
        {
            CurrentPage = target;
        }
    }

    partial void OnCurrentPageChanged(AppPage value)
    {
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(CurrentViewModel));

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
