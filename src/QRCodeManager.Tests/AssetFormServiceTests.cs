using QRCodeManager.Application.Constants;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class AssetFormServiceTests
{
    private readonly AssetFormService _sut = new(new TestSettingsService());

    [Fact]
    public void FormatDisplay_ValidForm_ReturnsFormattedText()
    {
        var form = CreateSampleForm();
        var output = _sut.FormatDisplay(form);

        Assert.Contains("📦 Eser Bilgisi", output);
        Assert.Contains("Ürün      : Laptop", output);
        Assert.Contains("Seri No   : ABC123456", output);
    }

    [Fact]
    public void ParseFromContent_FormattedText_ReturnsFormFields()
    {
        const string content = """
            📦 Eser Bilgisi
            Ürün      : Laptop
            Materyal  : Alüminyum
            Sahibi    : Yunus Emre Teke
            Konumu    : Sivas Halk Eğitim Merkezi
            Seri No   : ABC123456
            """;

        var form = _sut.ParseFromContent(content);

        Assert.Equal("Laptop", form.GetValue("urun"));
        Assert.Equal("ABC123456", form.GetValue("seriNo"));
    }

    [Fact]
    public void ToJson_ValidForm_ReturnsDynamicEserJson()
    {
        var json = _sut.ToJson(CreateSampleForm());

        Assert.Contains("ABC123456", json);
        Assert.Contains("Laptop", json);
        Assert.Contains("fields", json);
        Assert.Contains("schemaVersion", json);
    }

    [Fact]
    public void TryValidate_MissingRequiredFields_ReturnsFalse()
    {
        var isValid = _sut.TryValidate(new AssetFormDto(), out var error);

        Assert.False(isValid);
        Assert.Contains("zorunlu", error, StringComparison.OrdinalIgnoreCase);
    }

    private static AssetFormDto CreateSampleForm()
    {
        var form = new AssetFormDto();
        form.SetValue("urun", "Laptop");
        form.SetValue("materyal", "Alüminyum");
        form.SetValue("sahibi", "Yunus Emre Teke");
        form.SetValue("konumu", "Sivas Halk Eğitim Merkezi");
        form.SetValue("seriNo", "ABC123456");
        return form;
    }

    private sealed class TestSettingsService : ISettingsService
    {
        private AppSettings _settings = new() { FieldDefinitions = FieldDefinitionDefaults.Create() };

        public AppSettings GetSettings() => _settings;

        public Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            _settings = settings;
            return Task.CompletedTask;
        }
    }
}
