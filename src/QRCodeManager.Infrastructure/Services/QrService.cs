using System.Drawing;
using Microsoft.Extensions.Logging;
using QRCoder;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace QRCodeManager.Infrastructure.Services;

public class QrService : IQrService
{
    private const int DefaultPixelsPerModule = 8;

    private readonly IJsonService _jsonService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<QrService> _logger;

    public QrService(
        IJsonService jsonService,
        ISettingsService settingsService,
        ILogger<QrService> logger)
    {
        _jsonService = jsonService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public byte[] GenerateQr(string content, QrErrorCorrectionLevel errorCorrectionLevel = QrErrorCorrectionLevel.M)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new QrProcessingException("QR içeriği boş olamaz.");
        }

        _jsonService.ValidateSize(content, _settingsService.GetSettings().MaximumJsonSize);

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, MapErrorCorrection(errorCorrectionLevel));
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(DefaultPixelsPerModule);
        }
        catch (QrProcessingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodu oluşturulurken hata oluştu.");
            throw new QrProcessingException("QR kodu oluşturulamadı.", ex);
        }
    }

    public string DecodeQr(byte[] imageBytes)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            throw new QrProcessingException("QR görüntüsü boş.");
        }

        try
        {
            using var stream = new MemoryStream(imageBytes);
            using var bitmap = new Bitmap(stream);
            return DecodeBitmap(bitmap);
        }
        catch (QrProcessingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR kodu çözümlenirken hata oluştu.");
            throw new QrProcessingException("QR kodu çözümlenemedi.", ex);
        }
    }

    public string DecodeQrFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new QrProcessingException("QR görüntü dosyası bulunamadı.");
        }

        try
        {
            using var bitmap = new Bitmap(filePath);
            return DecodeBitmap(bitmap);
        }
        catch (QrProcessingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "QR dosyası okunurken hata oluştu: {FilePath}", filePath);
            throw new QrProcessingException("QR dosyası okunamadı.", ex);
        }
    }

    private string DecodeBitmap(Bitmap bitmap)
    {
        var reader = new BarcodeReader
        {
            Options = new DecodingOptions
            {
                TryHarder = true,
                CharacterSet = "UTF-8"
            },
            AutoRotate = true
        };

        var result = reader.Decode(bitmap);
        if (result is null || string.IsNullOrWhiteSpace(result.Text))
        {
            throw new QrProcessingException("Görüntüde geçerli bir QR kodu bulunamadı.");
        }

        return result.Text;
    }

    private static QRCodeGenerator.ECCLevel MapErrorCorrection(QrErrorCorrectionLevel level) =>
        level switch
        {
            QrErrorCorrectionLevel.L => QRCodeGenerator.ECCLevel.L,
            QrErrorCorrectionLevel.M => QRCodeGenerator.ECCLevel.M,
            QrErrorCorrectionLevel.Q => QRCodeGenerator.ECCLevel.Q,
            QrErrorCorrectionLevel.H => QRCodeGenerator.ECCLevel.H,
            _ => QRCodeGenerator.ECCLevel.M
        };
}
