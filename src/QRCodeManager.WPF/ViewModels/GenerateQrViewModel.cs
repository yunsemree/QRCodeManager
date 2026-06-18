using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;
using QRCodeManager.WPF.Helpers;
using QRCodeManager.WPF.Models;

namespace QRCodeManager.WPF.ViewModels;

public partial class GenerateQrViewModel : ObservableObject
{
    private readonly IQrService _qrService;
    private readonly IJsonService _jsonService;
    private readonly IAssetFormService _assetFormService;
    private readonly IHistoryRepository _historyRepository;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<GenerateQrViewModel> _logger;

    [ObservableProperty]
    private string _urun = "Laptop";

    [ObservableProperty]
    private string _materyal = "Alüminyum";

    [ObservableProperty]
    private string _sahibi = "Yunus Emre Teke";

    [ObservableProperty]
    private string _konumu = "Sivas Halk Eğitim Merkezi";

    [ObservableProperty]
    private string _seriNo = "ABC123456";

    [ObservableProperty]
    private bool _isFormValid;

    [ObservableProperty]
    private string _validationMessage = "Zorunlu alanları doldurun.";

    [ObservableProperty]
    private BitmapImage? _qrPreview;

    [ObservableProperty]
    private byte[]? _qrImageBytes;

    [ObservableProperty]
    private ErrorCorrectionOption? _selectedErrorCorrection;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IReadOnlyList<ErrorCorrectionOption> ErrorCorrectionOptions { get; } = ErrorCorrectionOption.All;

    public GenerateQrViewModel(
        IQrService qrService,
        IJsonService jsonService,
        IAssetFormService assetFormService,
        IHistoryRepository historyRepository,
        ISettingsService settingsService,
        ILogger<GenerateQrViewModel> logger)
    {
        _qrService = qrService;
        _jsonService = jsonService;
        _assetFormService = assetFormService;
        _historyRepository = historyRepository;
        _settingsService = settingsService;
        _logger = logger;

        var defaultLevel = settingsService.GetSettings().DefaultErrorCorrection;
        SelectedErrorCorrection = ErrorCorrectionOptions.First(x => x.Level == defaultLevel);
        ValidateForm();
    }

    partial void OnUrunChanged(string value) => ValidateForm();
    partial void OnSeriNoChanged(string value) => ValidateForm();

    public void LoadJson(string json)
    {
        var form = _assetFormService.ParseFromContent(json);
        Urun = form.Urun;
        Materyal = form.Materyal;
        Sahibi = form.Sahibi;
        Konumu = form.Konumu;
        SeriNo = form.SeriNo;
        ValidateForm();
    }

    [RelayCommand]
    private void ClearForm()
    {
        Urun = string.Empty;
        Materyal = string.Empty;
        Sahibi = string.Empty;
        Konumu = string.Empty;
        SeriNo = string.Empty;
        QrPreview = null;
        QrImageBytes = null;
        StatusMessage = "Form temizlendi.";
        ValidateForm();
    }

    [RelayCommand]
    private async Task GenerateQrAsync()
    {
        try
        {
            var form = BuildForm();
            if (!_assetFormService.TryValidate(form, out var validationError))
            {
                StatusMessage = validationError;
                return;
            }

            var qrContent = _assetFormService.ToQrContent(form);
            var json = _assetFormService.ToJson(form);
            var settings = _settingsService.GetSettings();
            _jsonService.ValidateSize(qrContent, settings.MaximumJsonSize);

            var level = SelectedErrorCorrection?.Level ?? QrErrorCorrectionLevel.M;
            QrImageBytes = _qrService.GenerateQr(qrContent, level);
            QrPreview = ImageHelper.ToBitmapImage(QrImageBytes);

            var fileName = $"qr_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var imagePath = Path.Combine(AppPaths.QrImagesDirectory, fileName);
            await ClipboardHelper.SavePngAsync(QrImageBytes, imagePath);

            await _historyRepository.AddAsync(new QrHistoryDto
            {
                Title = string.IsNullOrWhiteSpace(form.Urun) ? "QR Kaydı" : form.Urun.Trim(),
                JsonData = json,
                QrImagePath = imagePath,
                CreatedDate = DateTime.UtcNow,
                QrType = QrType.Generated
            });

            StatusMessage = "QR kodu oluşturuldu ve geçmişe kaydedildi.";
        }
        catch (Exception ex) when (ex is QrProcessingException or InvalidOperationException or ArgumentException)
        {
            _logger.LogError(ex, "QR oluşturma hatası");
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUseQrImage))]
    private async Task SavePngAsync()
    {
        if (QrImageBytes is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PNG Dosyası|*.png",
            FileName = $"qr_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        };

        if (dialog.ShowDialog() == true)
        {
            await ClipboardHelper.SavePngAsync(QrImageBytes, dialog.FileName);
            StatusMessage = "PNG kaydedildi.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanUseQrImage))]
    private void CopyImage()
    {
        if (QrPreview is not null)
        {
            ClipboardHelper.CopyImage(QrPreview);
            StatusMessage = "Görüntü panoya kopyalandı.";
        }
    }

    private bool CanUseQrImage() => QrImageBytes is { Length: > 0 };

    partial void OnQrImageBytesChanged(byte[]? value)
    {
        SavePngCommand.NotifyCanExecuteChanged();
        CopyImageCommand.NotifyCanExecuteChanged();
    }

    private void ValidateForm()
    {
        var form = BuildForm();
        IsFormValid = _assetFormService.TryValidate(form, out var error);
        ValidationMessage = IsFormValid ? "Form hazır" : error;
    }

    private AssetFormDto BuildForm() =>
        new()
        {
            Urun = Urun,
            Materyal = Materyal,
            Sahibi = Sahibi,
            Konumu = Konumu,
            SeriNo = SeriNo
        };
}
