using System.Text.Json;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.Exceptions;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Application.Validators;

namespace QRCodeManager.Infrastructure.Services;

public class JsonService : IJsonService
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger<JsonService> _logger;

    public JsonService(ILogger<JsonService> logger)
    {
        _logger = logger;
    }

    public bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Geçersiz JSON doğrulaması başarısız.");
            return false;
        }
    }

    public string FormatJson(string json)
    {
        ValidateInput(json);

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            return JsonSerializer.Serialize(document, WriteOptions);
        }
        catch (JsonException ex)
        {
            throw new QrProcessingException("JSON biçimlendirilemedi.", ex);
        }
    }

    public string MinifyJson(string json)
    {
        ValidateInput(json);

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            return JsonSerializer.Serialize(document);
        }
        catch (JsonException ex)
        {
            throw new QrProcessingException("JSON sıkıştırılamadı.", ex);
        }
    }

    public void ValidateSize(string json, int maxSize)
    {
        JsonSizeValidator.Validate(json, maxSize);
    }

    private static void ValidateInput(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON içeriği boş olamaz.", nameof(json));
        }
    }
}
