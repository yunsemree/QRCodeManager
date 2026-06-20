using Microsoft.Extensions.Logging.Abstractions;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Enums;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class QrGenerationTests
{
    private readonly QrService _sut;
    private readonly AssetFormService _assetFormService;

    public QrGenerationTests()
    {
        var settingsService = TestData.CreateSettingsService();
        var jsonService = new JsonService(NullLogger<JsonService>.Instance);
        _assetFormService = new AssetFormService(settingsService);
        _sut = new QrService(jsonService, settingsService, NullLogger<QrService>.Instance);
    }

    [Fact]
    public void GenerateQr_FormattedContent_ReturnsPngBytes()
    {
        var content = _assetFormService.FormatDisplay(new Application.DTOs.AssetFormDto
        {
            Values =
            {
                ["urun"] = "Laptop",
                ["seriNo"] = "ABC123456"
            }
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
        var content = _assetFormService.FormatDisplay(TestData.CreateSampleForm());
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
