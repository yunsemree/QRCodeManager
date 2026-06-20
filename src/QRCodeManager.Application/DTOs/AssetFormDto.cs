namespace QRCodeManager.Application.DTOs;

public class AssetFormDto
{
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetValue(string key) =>
        Values.TryGetValue(key, out var value) ? value : string.Empty;

    public void SetValue(string key, string value) =>
        Values[key] = value.Trim();
}
