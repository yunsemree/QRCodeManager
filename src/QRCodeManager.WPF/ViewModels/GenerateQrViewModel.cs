using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Application.Helpers;
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
    private bool _isReloadingFields;
    private CancellationTokenSource? _persistCts;

    public ObservableCollection<DynamicFormField> Fields { get; } = [];

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
    private QrContentTypeOption? _selectedContentType;

    [ObservableProperty]
    private string _urlContent = string.Empty;

    [ObservableProperty]
    private string _plainTextContent = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IReadOnlyList<ErrorCorrectionOption> ErrorCorrectionOptions { get; } = ErrorCorrectionOption.All;
    public IReadOnlyList<QrContentTypeOption> ContentTypeOptions { get; } = QrContentTypeOption.All;

    public bool IsAssetFormContent => SelectedContentType?.Type == QrContentType.AssetForm;
    public bool IsUrlContent => SelectedContentType?.Type == QrContentType.Url;
    public bool IsPlainTextContent => SelectedContentType?.Type == QrContentType.PlainText;

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

        var settings = settingsService.GetSettings();
        SelectedErrorCorrection = ErrorCorrectionOptions.First(x => x.Level == settings.DefaultErrorCorrection);
        SelectedContentType = ContentTypeOptions.First(x => x.Type == settings.DefaultQrContentType);
        ReloadFields();
    }

    public void ReloadFields(Dictionary<string, string>? values = null)
    {
        _isReloadingFields = true;
        UnsubscribeAllFields();
        Fields.Clear();

        var definitions = _settingsService.GetSettings().FieldDefinitions
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Label, StringComparer.CurrentCultureIgnoreCase);

        foreach (var definition in definitions)
        {
            var existingValue = string.Empty;
            if (values is not null && values.TryGetValue(definition.Key, out var value))
            {
                existingValue = value;
            }

            var field = new DynamicFormField(
                definition.Key,
                definition.Label,
                definition.IsRequired,
                existingValue);
            SubscribeField(field);
            Fields.Add(field);
        }

        _isReloadingFields = false;
        ValidateForm();
    }

    public void LoadJson(string json)
    {
        var contentType = QrGenerationPayloadSerializer.GetContentType(json);
        SelectedContentType = ContentTypeOptions.First(x => x.Type == contentType);

        switch (contentType)
        {
            case QrContentType.Url:
                UrlContent = QrGenerationPayloadSerializer.GetSimpleContent(json) ?? string.Empty;
                break;
            case QrContentType.PlainText:
                PlainTextContent = QrGenerationPayloadSerializer.GetSimpleContent(json) ?? string.Empty;
                break;
            default:
                var form = _assetFormService.ParseFromContent(json);
                ReloadFields(form.Values.ToDictionary(pair => pair.Key, pair => pair.Value));
                break;
        }

        ValidateForm();
    }

    [RelayCommand]
    private async Task AddFieldAsync()
    {
        var nextOrder = Fields.Count == 0 ? 1 : Fields.Count + 1;
        var field = new DynamicFormField($"alan{nextOrder}", "Yeni Alan", false);
        SubscribeField(field);
        Fields.Add(field);
        await PersistFieldDefinitionsAsync();
        ValidateForm();
        StatusMessage = "Alan eklendi.";
    }

    [RelayCommand]
    private async Task RemoveFieldAsync(DynamicFormField? field)
    {
        if (field is null)
        {
            return;
        }

        UnsubscribeField(field);
        Fields.Remove(field);
        await PersistFieldDefinitionsAsync();
        ValidateForm();
        StatusMessage = "Alan kaldırıldı.";
    }

    [RelayCommand]
    private void ClearForm()
    {
        if (IsAssetFormContent)
        {
            ReloadFields();
        }
        else if (IsUrlContent)
        {
            UrlContent = string.Empty;
        }
        else
        {
            PlainTextContent = string.Empty;
        }

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
            if (!TryBuildQrPayload(out var qrContent, out var historyJson, out var title, out var validationError))
            {
                StatusMessage = validationError;
                return;
            }

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
                Title = title,
                JsonData = historyJson,
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

    partial void OnSelectedContentTypeChanged(QrContentTypeOption? value)
    {
        OnPropertyChanged(nameof(IsAssetFormContent));
        OnPropertyChanged(nameof(IsUrlContent));
        OnPropertyChanged(nameof(IsPlainTextContent));
        ValidateForm();
        _ = PersistDefaultContentTypeAsync();
    }

    partial void OnUrlContentChanged(string value) => ValidateForm();

    partial void OnPlainTextContentChanged(string value) => ValidateForm();

    private void ValidateForm()
    {
        IsFormValid = TryBuildQrPayload(out _, out _, out _, out var error);
        ValidationMessage = IsFormValid ? "Form hazır" : error;
    }

    private bool TryBuildQrPayload(
        out string qrContent,
        out string historyJson,
        out string title,
        out string validationError)
    {
        qrContent = string.Empty;
        historyJson = string.Empty;
        title = "QR Kaydı";
        validationError = string.Empty;

        switch (SelectedContentType?.Type ?? QrContentType.AssetForm)
        {
            case QrContentType.Url:
                return TryBuildUrlPayload(out qrContent, out historyJson, out title, out validationError);
            case QrContentType.PlainText:
                return TryBuildPlainTextPayload(out qrContent, out historyJson, out title, out validationError);
            default:
                return TryBuildAssetFormPayload(out qrContent, out historyJson, out title, out validationError);
        }
    }

    private bool TryBuildAssetFormPayload(
        out string qrContent,
        out string historyJson,
        out string title,
        out string validationError)
    {
        qrContent = string.Empty;
        historyJson = string.Empty;
        title = "QR Kaydı";
        validationError = string.Empty;

        var form = BuildForm();
        if (!_assetFormService.TryValidate(form, out validationError))
        {
            return false;
        }

        qrContent = _assetFormService.ToQrContent(form);
        historyJson = QrGenerationPayloadSerializer.SerializeAssetForm(_assetFormService.ToJson(form));

        var titleField = Fields.FirstOrDefault(f => f.IsRequired)?.Value
            ?? Fields.FirstOrDefault()?.Value
            ?? "QR Kaydı";
        title = string.IsNullOrWhiteSpace(titleField) ? "QR Kaydı" : titleField.Trim();
        return true;
    }

    private bool TryBuildUrlPayload(
        out string qrContent,
        out string historyJson,
        out string title,
        out string validationError)
    {
        qrContent = string.Empty;
        historyJson = string.Empty;
        title = "QR Kaydı";
        validationError = string.Empty;

        var url = UrlContent.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            validationError = "URL zorunludur.";
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            validationError = "Geçerli bir http veya https URL girin.";
            return false;
        }

        qrContent = url;
        historyJson = QrGenerationPayloadSerializer.SerializeUrl(url);
        title = url.Length > 60 ? $"{url[..57]}..." : url;
        return true;
    }

    private bool TryBuildPlainTextPayload(
        out string qrContent,
        out string historyJson,
        out string title,
        out string validationError)
    {
        qrContent = string.Empty;
        historyJson = string.Empty;
        title = "QR Kaydı";
        validationError = string.Empty;

        var text = PlainTextContent.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            validationError = "Metin içeriği zorunludur.";
            return false;
        }

        qrContent = text;
        historyJson = QrGenerationPayloadSerializer.SerializePlainText(text);
        title = text.Length > 60 ? $"{text[..57]}..." : text;
        return true;
    }

    private AssetFormDto BuildForm()
    {
        var form = new AssetFormDto();
        foreach (var field in Fields)
        {
            form.SetValue(field.Key, field.Value);
        }

        return form;
    }

    private void SubscribeField(DynamicFormField field) =>
        field.PropertyChanged += OnFieldPropertyChanged;

    private void UnsubscribeField(DynamicFormField field) =>
        field.PropertyChanged -= OnFieldPropertyChanged;

    private void UnsubscribeAllFields()
    {
        foreach (var field in Fields.ToList())
        {
            UnsubscribeField(field);
        }
    }

    private void OnFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ValidateForm();

        if (_isReloadingFields || sender is not DynamicFormField)
        {
            return;
        }

        if (e.PropertyName is nameof(DynamicFormField.Key)
            or nameof(DynamicFormField.Label)
            or nameof(DynamicFormField.IsRequired))
        {
            SchedulePersistFieldDefinitions();
        }
    }

    private void SchedulePersistFieldDefinitions()
    {
        _persistCts?.Cancel();
        _persistCts = new CancellationTokenSource();
        var token = _persistCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(400, token);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await PersistFieldDefinitionsAsync();
                });
            }
            catch (TaskCanceledException)
            {
                // Yeni değişiklik geldiğinde bekleyen kayıt iptal edilir.
            }
        }, token);
    }

    private async Task PersistFieldDefinitionsAsync()
    {
        try
        {
            var settings = _settingsService.GetSettings();
            settings.FieldDefinitions = Fields.Select((field, index) => new FieldDefinitionDto
            {
                Key = field.Key.Trim(),
                Label = field.Label.Trim(),
                IsRequired = field.IsRequired,
                SortOrder = index
            }).ToList();

            await _settingsService.SaveSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alan tanımları kaydedilemedi");
            StatusMessage = "Alan tanımları kaydedilemedi.";
        }
    }

    private async Task PersistDefaultContentTypeAsync()
    {
        if (SelectedContentType is null)
        {
            return;
        }

        try
        {
            var settings = _settingsService.GetSettings();
            settings.DefaultQrContentType = SelectedContentType.Type;
            await _settingsService.SaveSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Varsayılan içerik tipi kaydedilemedi");
        }
    }
}
