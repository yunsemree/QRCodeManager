using QRCodeManager.Domain.Enums;

namespace QRCodeManager.WPF.Models;

public sealed class ErrorCorrectionOption
{
    public QrErrorCorrectionLevel Level { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public static IReadOnlyList<ErrorCorrectionOption> All { get; } =
    [
        new() { Level = QrErrorCorrectionLevel.L, DisplayName = "Düşük (L)" },
        new() { Level = QrErrorCorrectionLevel.M, DisplayName = "Orta (M)" },
        new() { Level = QrErrorCorrectionLevel.Q, DisplayName = "Yüksek (Q)" },
        new() { Level = QrErrorCorrectionLevel.H, DisplayName = "Çok Yüksek (H)" }
    ];
}
