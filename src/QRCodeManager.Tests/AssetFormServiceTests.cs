using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class AssetFormServiceTests
{
    private readonly AssetFormService _sut = new();

    [Fact]
    public void FormatDisplay_ValidForm_ReturnsFormattedText()
    {
        var output = _sut.FormatDisplay(new Application.DTOs.AssetFormDto
        {
            Urun = "Laptop",
            Materyal = "Alüminyum",
            Sahibi = "Yunus Emre Teke",
            Konumu = "Sivas Halk Eğitim Merkezi",
            SeriNo = "ABC123456"
        });

        Assert.Contains("📦 Eser Bilgisi", output);
        Assert.Contains("Ürün      : Laptop", output);
        Assert.Contains("Materyal  : Alüminyum", output);
        Assert.Contains("Sahibi    : Yunus Emre Teke", output);
        Assert.Contains("Konumu    : Sivas Halk Eğitim Merkezi", output);
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

        Assert.Equal("Laptop", form.Urun);
        Assert.Equal("Alüminyum", form.Materyal);
        Assert.Equal("Yunus Emre Teke", form.Sahibi);
        Assert.Equal("Sivas Halk Eğitim Merkezi", form.Konumu);
        Assert.Equal("ABC123456", form.SeriNo);
    }

    [Fact]
    public void ToJson_ValidForm_ReturnsEserJson()
    {
        var json = _sut.ToJson(new Application.DTOs.AssetFormDto
        {
            SeriNo = "ABC123456",
            Urun = "Laptop"
        });

        Assert.Contains("ABC123456", json);
        Assert.Contains("Laptop", json);
        Assert.Contains("eser", json);
    }

    [Fact]
    public void TryValidate_MissingRequiredFields_ReturnsFalse()
    {
        var isValid = _sut.TryValidate(new Application.DTOs.AssetFormDto(), out var error);

        Assert.False(isValid);
        Assert.Contains("Seri numarası", error);
    }
}
