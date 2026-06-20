using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.WPF.Helpers;

namespace QRCodeManager.WPF.ViewModels;

public partial class ReadQrViewModel : ObservableObject
{
    private readonly IQrService _qrService;
    private readonly IAssetFormService _assetFormService;
    private readonly ILogger<ReadQrViewModel> _logger;

    [ObservableProperty]
    private string _selectedImagePath = string.Empty;

    [ObservableProperty]
    private BitmapImage? _loadedImage;

    [ObservableProperty]
    private string _formattedOutput = string.Empty;

    [ObservableProperty]
    private bool _hasDecodedData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReadQrViewModel(
        IQrService qrService,
        IAssetFormService assetFormService,
        ILogger<ReadQrViewModel> logger)
    {
        _qrService = qrService;
        _assetFormService = assetFormService;
        _logger = logger;
    }

    [RelayCommand]
    private void OpenImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Görüntü Dosyaları|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Tüm Dosyalar|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SelectedImagePath = dialog.FileName;
        LoadedImage = ImageHelper.ToBitmapImageFromFile(SelectedImagePath);
        DecodeQrCommand.Execute(null);
    }

    [RelayCommand(CanExecute = nameof(CanDecode))]
    private void DecodeQr()
    {
        try
        {
            var rawText = _qrService.DecodeQrFromFile(SelectedImagePath);
            var form = _assetFormService.ParseFromContent(rawText);
            FormattedOutput = _assetFormService.FormatDisplay(form);
            HasDecodedData = form.Values.Values.Any(value => !string.IsNullOrWhiteSpace(value));

            StatusMessage = HasDecodedData
                ? "QR kodu başarıyla okundu."
                : "QR okundu ancak eser bilgisi bulunamadı.";
        }
        catch (Application.Exceptions.QrProcessingException ex)
        {
            _logger.LogWarning(ex, "QR çözümleme hatası");
            FormattedOutput = string.Empty;
            HasDecodedData = false;
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopyInfo))]
    private void CopyInfo()
    {
        ClipboardHelper.CopyText(FormattedOutput);
        StatusMessage = "Eser bilgisi panoya kopyalandı.";
    }

    private bool CanDecode() => !string.IsNullOrWhiteSpace(SelectedImagePath);
    private bool CanCopyInfo() => HasDecodedData;

    partial void OnSelectedImagePathChanged(string value) => DecodeQrCommand.NotifyCanExecuteChanged();
    partial void OnHasDecodedDataChanged(bool value) => CopyInfoCommand.NotifyCanExecuteChanged();
}
