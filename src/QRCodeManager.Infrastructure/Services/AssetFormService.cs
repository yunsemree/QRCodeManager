using System.Text;
using System.Text.Json;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.Infrastructure.Services;

public class AssetFormService : IAssetFormService
{
    private const int LabelWidth = 10;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ToJson(AssetFormDto form)
    {
        var payload = new
        {
            eser = new
            {
                urun = form.Urun.Trim(),
                materyal = form.Materyal.Trim(),
                sahibi = form.Sahibi.Trim(),
                konumu = form.Konumu.Trim(),
                seriNo = form.SeriNo.Trim()
            }
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    public string ToQrContent(AssetFormDto form) => FormatDisplay(form);

    public string FormatDisplay(AssetFormDto form)
    {
        var builder = new StringBuilder();
        builder.AppendLine("📦 Eser Bilgisi");
        builder.AppendLine(FormatLine("Ürün", form.Urun));
        builder.AppendLine(FormatLine("Materyal", form.Materyal));
        builder.AppendLine(FormatLine("Sahibi", form.Sahibi));
        builder.AppendLine(FormatLine("Konumu", form.Konumu));
        builder.Append(FormatLine("Seri No", form.SeriNo));
        return builder.ToString();
    }

    public AssetFormDto ParseFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new AssetFormDto();
        }

        var trimmed = content.Trim();
        if (trimmed.StartsWith('{'))
        {
            return ParseFromJson(trimmed);
        }

        return ParseFromDisplayText(trimmed);
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
                MapEserProperties(eser, form);
                return form;
            }

            if (document.RootElement.TryGetProperty("asset", out var asset))
            {
                form.SeriNo = GetString(asset, "id");
                form.Urun = GetString(asset, "name");
                form.Materyal = GetString(asset, "brand");
                if (string.IsNullOrWhiteSpace(form.Materyal))
                {
                    form.Materyal = GetString(asset, "model");
                }

                form.Sahibi = GetString(asset, "owner");
                form.Konumu = GetString(asset, "location");
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
        if (string.IsNullOrWhiteSpace(form.SeriNo))
        {
            errorMessage = "Seri numarası zorunludur.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(form.Urun))
        {
            errorMessage = "Ürün adı zorunludur.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static AssetFormDto ParseFromDisplayText(string text)
    {
        var form = new AssetFormDto();

        foreach (var line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var label = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            switch (label)
            {
                case "Ürün":
                    form.Urun = value;
                    break;
                case "Materyal":
                    form.Materyal = value;
                    break;
                case "Sahibi":
                    form.Sahibi = value;
                    break;
                case "Konumu":
                    form.Konumu = value;
                    break;
                case "Seri No":
                    form.SeriNo = value;
                    break;
            }
        }

        return form;
    }

    private static void MapEserProperties(JsonElement eser, AssetFormDto form)
    {
        form.Urun = GetString(eser, "urun");
        form.Materyal = GetString(eser, "materyal");
        form.Sahibi = GetString(eser, "sahibi");
        form.Konumu = GetString(eser, "konumu");
        form.SeriNo = GetString(eser, "seriNo");
    }

    private static string FormatLine(string label, string value) =>
        $"{label.PadRight(LabelWidth)}: {value.Trim()}";

    private static string GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) ? value.GetString() ?? string.Empty : string.Empty;
}
