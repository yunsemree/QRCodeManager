using QRCodeManager.Domain.Enums;

namespace QRCodeManager.Application.Interfaces;

public interface IQrService
{
    byte[] GenerateQr(string json, QrErrorCorrectionLevel errorCorrectionLevel = QrErrorCorrectionLevel.M);
    string DecodeQr(byte[] imageBytes);
    string DecodeQrFromFile(string filePath);
}
