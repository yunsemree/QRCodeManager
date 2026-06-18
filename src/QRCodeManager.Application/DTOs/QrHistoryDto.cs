using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Application.DTOs;

public class QrHistoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;
    public string? QrImagePath { get; set; }
    public DateTime CreatedDate { get; set; }
    public QrType QrType { get; set; }
    public byte[]? PreviewImage { get; set; }
}
