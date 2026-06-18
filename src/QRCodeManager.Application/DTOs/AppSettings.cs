using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Application.DTOs;

public class AppSettings
{
    public string Theme { get; set; } = "Light";
    public QrErrorCorrectionLevel DefaultErrorCorrection { get; set; } = QrErrorCorrectionLevel.M;
    public string DefaultExportFormat { get; set; } = "PNG";
    public int MaximumJsonSize { get; set; } = 4096;
}
