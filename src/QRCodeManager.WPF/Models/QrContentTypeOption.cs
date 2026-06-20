using QRCodeManager.Domain.Enums;

namespace QRCodeManager.WPF.Models;

public sealed class QrContentTypeOption
{
    public QrContentType Type { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public static IReadOnlyList<QrContentTypeOption> All { get; } =
    [
        new() { Type = QrContentType.AssetForm, DisplayName = "Eser Bilgisi (Form)" },
        new() { Type = QrContentType.Url, DisplayName = "URL" },
        new() { Type = QrContentType.PlainText, DisplayName = "Metin" }
    ];
}
