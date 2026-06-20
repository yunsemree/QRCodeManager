using QRCodeManager.Application.Helpers;
using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Tests;

public class QrGenerationPayloadSerializerTests
{
    [Fact]
    public void SerializeUrl_IncludesContentTypeAndContent()
    {
        var json = QrGenerationPayloadSerializer.SerializeUrl("https://example.com");

        Assert.Contains("\"contentType\":1", json.Replace(" ", string.Empty));
        Assert.Contains("https://example.com", json);
        Assert.Equal(QrContentType.Url, QrGenerationPayloadSerializer.GetContentType(json));
        Assert.Equal("https://example.com", QrGenerationPayloadSerializer.GetSimpleContent(json));
    }

    [Fact]
    public void SerializePlainText_IncludesContentTypeAndContent()
    {
        var json = QrGenerationPayloadSerializer.SerializePlainText("Merhaba dünya");

        Assert.Equal(QrContentType.PlainText, QrGenerationPayloadSerializer.GetContentType(json));
        Assert.Equal("Merhaba dünya", QrGenerationPayloadSerializer.GetSimpleContent(json));
    }

    [Fact]
    public void GetContentType_LegacyEserJson_ReturnsAssetForm()
    {
        const string json = """{"eser":{"schemaVersion":2,"fields":{"urun":"Laptop"}}}""";

        Assert.Equal(QrContentType.AssetForm, QrGenerationPayloadSerializer.GetContentType(json));
    }
}
