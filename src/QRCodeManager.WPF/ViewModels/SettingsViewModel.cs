using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;

namespace QRCodeManager.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsViewModel> _logger;
    private bool _isLoading;

    [ObservableProperty]
    private string _theme = "Açık";

    [ObservableProperty]
    private QrErrorCorrectionLevel _defaultErrorCorrection = QrErrorCorrectionLevel.M;

    [ObservableProperty]
    private string _defaultExportFormat = "PNG";

    [ObservableProperty]
    private int _maximumJsonSize = 4096;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IReadOnlyList<string> Themes { get; } = ["Açık", "Koyu"];
    public IReadOnlyList<string> ExportFormats { get; } = ["PNG", "JPEG"];

    public event Action<string>? ThemeChanged;

    public SettingsViewModel(ISettingsService settingsService, ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoading = true;
        var settings = _settingsService.GetSettings();
        Theme = MapThemeToDisplay(settings.Theme);
        DefaultErrorCorrection = settings.DefaultErrorCorrection;
        DefaultExportFormat = settings.DefaultExportFormat;
        MaximumJsonSize = settings.MaximumJsonSize;
        _isLoading = false;
    }

    partial void OnThemeChanged(string value)
    {
        if (_isLoading)
        {
            return;
        }

        var storageTheme = MapThemeToStorage(value);
        ThemeChanged?.Invoke(storageTheme);
        StatusMessage = string.Equals(storageTheme, "Dark", StringComparison.OrdinalIgnoreCase)
            ? "Koyu tema uygulandı."
            : "Açık tema uygulandı.";
        _ = PersistThemeAsync(storageTheme);
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            await SaveAllSettingsAsync();
            StatusMessage = "Ayarlar kaydedildi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar kaydedilemedi");
            StatusMessage = "Ayarlar kaydedilemedi.";
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        Theme = "Açık";
        DefaultErrorCorrection = QrErrorCorrectionLevel.M;
        DefaultExportFormat = "PNG";
        MaximumJsonSize = 4096;
        StatusMessage = "Varsayılan ayarlar yüklendi.";
    }

    private async Task PersistThemeAsync(string storageTheme)
    {
        try
        {
            var settings = _settingsService.GetSettings();
            settings.Theme = storageTheme;
            await _settingsService.SaveSettingsAsync(settings);
            StatusMessage = "Tema kaydedildi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tema kaydedilemedi");
            StatusMessage = "Tema kaydedilemedi.";
        }
    }

    private async Task SaveAllSettingsAsync()
    {
        var current = _settingsService.GetSettings();
        var settings = new AppSettings
        {
            Theme = MapThemeToStorage(Theme),
            DefaultErrorCorrection = DefaultErrorCorrection,
            DefaultExportFormat = DefaultExportFormat,
            MaximumJsonSize = MaximumJsonSize,
            FieldDefinitions = current.FieldDefinitions,
            DefaultQrContentType = current.DefaultQrContentType,
            IsAuthSkipped = current.IsAuthSkipped,
            CurrentUserId = current.CurrentUserId
        };

        await _settingsService.SaveSettingsAsync(settings);
        ThemeChanged?.Invoke(settings.Theme);
    }

    private static string MapThemeToDisplay(string theme) =>
        string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Koyu" : "Açık";

    private static string MapThemeToStorage(string theme) =>
        string.Equals(theme, "Koyu", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
}
