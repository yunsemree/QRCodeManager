using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Domain.Entities;

public class QrHistory
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;
    public string? QrImagePath { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public QrType QrType { get; set; } = QrType.Generated;
}
