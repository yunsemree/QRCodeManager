using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class QrDecodeTests
{
    private readonly QrService _sut;
    private readonly AssetFormService _assetFormService = new();

    public QrDecodeTests()
    {
        var jsonService = new JsonService(NullLogger<JsonService>.Instance);
        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(x => x.GetSettings()).Returns(new Application.DTOs.AppSettings
        {
            MaximumJsonSize = 4096
        });

        _sut = new QrService(jsonService, settingsService.Object, NullLogger<QrService>.Instance);
    }

    [Fact]
    public void DecodeQr_GeneratedImage_ReturnsOriginalContent()
    {
        var content = _assetFormService.FormatDisplay(new Application.DTOs.AssetFormDto
        {
            Urun = "Laptop",
            Materyal = "Alüminyum",
            Sahibi = "Yunus Emre Teke",
            Konumu = "Sivas Halk Eğitim Merkezi",
            SeriNo = "ABC123456"
        });

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
