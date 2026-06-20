using Microsoft.Extensions.Logging.Abstractions;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Domain.Enums;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class QrDecodeTests
{
    private readonly QrService _sut;
    private readonly AssetFormService _assetFormService;

    public QrDecodeTests()
    {
        var settingsService = TestData.CreateSettingsService();
        var jsonService = new JsonService(NullLogger<JsonService>.Instance);
        _assetFormService = new AssetFormService(settingsService);
        _sut = new QrService(jsonService, settingsService, NullLogger<QrService>.Instance);
    }

    [Fact]
    public void DecodeQr_GeneratedImage_ReturnsOriginalContent()
    {
        var content = _assetFormService.FormatDisplay(TestData.CreateSampleForm());
        var qrBytes = _sut.GenerateQr(content, QrErrorCorrectionLevel.M);
        var decoded = _sut.DecodeQr(qrBytes);

        Assert.Contains("ABC123456", decoded);
        Assert.Contains("Laptop", decoded);
        Assert.Contains("📦 Eser Bilgisi", decoded);
    }

    [Fact]
    public void DecodeQr_EmptyBytes_Throws()
    {
        Assert.Throws<QrProcessingException>(() => _sut.DecodeQr([]));
    }

    [Fact]
    public void DecodeQr_InvalidImage_Throws()
    {
        var invalidBytes = new byte[] { 1, 2, 3, 4 };
        Assert.Throws<QrProcessingException>(() => _sut.DecodeQr(invalidBytes));
    }
}
