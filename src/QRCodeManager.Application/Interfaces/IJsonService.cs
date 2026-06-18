namespace QRCodeManager.Application.Interfaces;

public interface IJsonService
{
    bool IsValidJson(string json);
    string FormatJson(string json);
    string MinifyJson(string json);
    void ValidateSize(string json, int maxSize);
}
