using System.Text.Json;
using System.Text.Json.Nodes;
using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Application.Helpers;

public static class QrGenerationPayloadSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string SerializeUrl(string url) =>
        JsonSerializer.Serialize(new { contentType = (int)QrContentType.Url, content = url.Trim() }, SerializerOptions);

    public static string SerializePlainText(string text) =>
        JsonSerializer.Serialize(new { contentType = (int)QrContentType.PlainText, content = text.Trim() }, SerializerOptions);

    public static string SerializeAssetForm(string eserJson)
    {
        var node = JsonNode.Parse(eserJson)!.AsObject();
        node["contentType"] = (int)QrContentType.AssetForm;
        return node.ToJsonString(SerializerOptions);
    }

    public static QrContentType GetContentType(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return QrContentType.AssetForm;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("contentType", out var contentType)
                && contentType.TryGetInt32(out var value)
                && Enum.IsDefined(typeof(QrContentType), value))
            {
                return (QrContentType)value;
            }

            if (document.RootElement.TryGetProperty("eser", out _))
            {
                return QrContentType.AssetForm;
            }
        }
        catch (JsonException)
        {
            // Eski veya geçersiz kayıtlar eser formu olarak ele alınır.
        }

        return QrContentType.AssetForm;
    }

    public static string? GetSimpleContent(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("content", out var content))
            {
                return content.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
