using Microsoft.Extensions.Logging.Abstractions;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Tests;

public class JsonServiceTests
{
    private readonly JsonService _sut = new(NullLogger<JsonService>.Instance);

    [Fact]
    public void IsValidJson_ValidObject_ReturnsTrue()
    {
        const string json = """{"name":"Test"}""";
        Assert.True(_sut.IsValidJson(json));
    }

    [Fact]
    public void IsValidJson_InvalidJson_ReturnsFalse()
    {
        Assert.False(_sut.IsValidJson("{invalid}"));
    }

    [Fact]
    public void FormatJson_ValidJson_ReturnsIndentedJson()
    {
        const string json = """{"asset":{"name":"Laptop"}}""";
        var result = _sut.FormatJson(json);
        Assert.Contains("\n", result);
        Assert.Contains("Laptop", result);
    }

    [Fact]
    public void MinifyJson_ValidJson_RemovesWhitespace()
    {
        const string json =
            """
            {
              "asset": {
                "name": "Laptop"
              }
            }
            """;
        var result = _sut.MinifyJson(json);
        Assert.DoesNotContain("\n", result);
        Assert.Contains("\"name\":\"Laptop\"", result.Replace(" ", string.Empty));
    }

    [Fact]
    public void ValidateSize_ExceedsLimit_Throws()
    {
        var json = new string('a', 20);
        Assert.Throws<InvalidOperationException>(() => _sut.ValidateSize(json, 10));
    }

    [Fact]
    public void FormatJson_InvalidJson_ThrowsQrProcessingException()
    {
        Assert.Throws<QrProcessingException>(() => _sut.FormatJson("{bad}"));
    }
}
