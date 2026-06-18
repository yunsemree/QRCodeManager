using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class QrGenerationTests
{
    private readonly QrService _sut;
    private readonly AssetFormService _assetFormService = new();

    public QrGenerationTests()
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
    public void GenerateQr_FormattedContent_ReturnsPngBytes()
    {
        var content = _assetFormService.FormatDisplay(new Application.DTOs.AssetFormDto
        {
            Urun = "Laptop",
            SeriNo = "ABC123456"
        });

        var result = _sut.GenerateQr(content, QrErrorCorrectionLevel.M);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        Assert.Equal(0x89, result[0]);
        Assert.Equal(0x50, result[1]);
    }

    [Fact]
    public void GenerateQr_TurkishCharacters_GeneratesSuccessfully()
    {
        var content = _assetFormService.FormatDisplay(new Application.DTOs.AssetFormDto
        {
            Urun = "Laptop",
            Materyal = "Alüminyum",
            Sahibi = "Yunus Emre Teke",
            Konumu = "Sivas Halk Eğitim Merkezi",
            SeriNo = "ABC123456"
        });

        var result = _sut.GenerateQr(content, QrErrorCorrectionLevel.H);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateQr_EmptyContent_Throws()
    {
        Assert.Throws<Application.Exceptions.QrProcessingException>(
            () => _sut.GenerateQr(string.Empty, QrErrorCorrectionLevel.L));
    }
}
