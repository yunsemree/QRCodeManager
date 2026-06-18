using QRCodeManager.Application.DTOs;

namespace QRCodeManager.Application.Interfaces;

public interface IAssetFormService
{
    string ToJson(AssetFormDto form);
    string ToQrContent(AssetFormDto form);
    string FormatDisplay(AssetFormDto form);
    AssetFormDto ParseFromJson(string json);
    AssetFormDto ParseFromContent(string content);
    bool TryValidate(AssetFormDto form, out string errorMessage);
}
