using System.Text;
using System.Text.Json;
using QRCodeManager.Application.Constants;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.Infrastructure.Services;

public class AssetFormService : IAssetFormService
{
    private readonly ISettingsService _settingsService;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AssetFormService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public string ToJson(AssetFormDto form)
    {
        var payload = new
        {
            eser = new
            {
                schemaVersion = 2,
                fields = BuildFieldPayload(form)
            }
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    public string ToQrContent(AssetFormDto form) => FormatDisplay(form);

    public string FormatDisplay(AssetFormDto form)
    {
        var definitions = GetDefinitions();
        var labelWidth = Math.Max(10, definitions.Max(d => d.Label.Length));
        var builder = new StringBuilder();
        builder.AppendLine("📦 Eser Bilgisi");

        for (var index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            var line = FormatLine(definition.Label, form.GetValue(definition.Key), labelWidth);
            builder.Append(index == definitions.Count - 1 ? line : line + Environment.NewLine);
        }

        return builder.ToString();
    }

    public AssetFormDto ParseFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new AssetFormDto();
        }

        var trimmed = content.Trim();
        return trimmed.StartsWith('{')
            ? ParseFromJson(trimmed)
            : ParseFromDisplayText(trimmed);
    }

    public AssetFormDto ParseFromJson(string json)
    {
        var form = new AssetFormDto();

        if (string.IsNullOrWhiteSpace(json))
        {
            return form;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("eser", out var eser))
            {
                if (eser.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in fields.EnumerateObject())
                    {
                        form.SetValue(property.Name, property.Value.GetString() ?? string.Empty);
                    }

                    return form;
                }

                MapLegacyEserProperties(eser, form);
                return form;
            }

            if (document.RootElement.TryGetProperty("asset", out var asset))
            {
                form.SetValue("seriNo", GetString(asset, "id"));
                form.SetValue("urun", GetString(asset, "name"));
                form.SetValue("materyal", GetString(asset, "brand"));
                if (string.IsNullOrWhiteSpace(form.GetValue("materyal")))
                {
                    form.SetValue("materyal", GetString(asset, "model"));
                }

                form.SetValue("sahibi", GetString(asset, "owner"));
                form.SetValue("konumu", GetString(asset, "location"));
            }
        }
        catch (JsonException)
        {
            // Geçersiz içerik boş forma döner.
        }

        return form;
    }

    public bool TryValidate(AssetFormDto form, out string errorMessage)
    {
        foreach (var definition in GetDefinitions().Where(d => d.IsRequired))
        {
            if (string.IsNullOrWhiteSpace(form.GetValue(definition.Key)))
            {
                errorMessage = $"{definition.Label} zorunludur.";
                return false;
            }
        }

        errorMessage = string.Empty;
        return true;
    }

    private IReadOnlyList<FieldDefinitionDto> GetDefinitions()
    {
        var definitions = _settingsService.GetSettings().FieldDefinitions;
        if (definitions is null || definitions.Count == 0)
        {
            return FieldDefinitionDefaults.Create();
        }

        return definitions
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Label, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static Dictionary<string, string> BuildFieldPayload(AssetFormDto form)
    {
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in form.Values)
        {
            payload[pair.Key] = pair.Value.Trim();
        }

        return payload;
    }

    private AssetFormDto ParseFromDisplayText(string text)
    {
        var form = new AssetFormDto();
        var labelMap = GetDefinitions().ToDictionary(
            d => d.Label,
            d => d.Key,
            StringComparer.CurrentCultureIgnoreCase);

        foreach (var line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("📦", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var label = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (labelMap.TryGetValue(label, out var key))
            {
                form.SetValue(key, value);
            }
        }

        return form;
    }

    private static void MapLegacyEserProperties(JsonElement eser, AssetFormDto form)
    {
        form.SetValue("urun", GetString(eser, "urun"));
        form.SetValue("materyal", GetString(eser, "materyal"));
        form.SetValue("sahibi", GetString(eser, "sahibi"));
        form.SetValue("konumu", GetString(eser, "konumu"));
        form.SetValue("seriNo", GetString(eser, "seriNo"));
    }

    private static string FormatLine(string label, string value, int labelWidth) =>
        $"{label.PadRight(labelWidth)}: {value.Trim()}";

    private static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) ? value.GetString() ?? string.Empty : string.Empty;
}
