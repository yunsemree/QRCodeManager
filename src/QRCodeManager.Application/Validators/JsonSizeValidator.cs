namespace QRCodeManager.Application.Validators;

public static class JsonSizeValidator
{
    public static void Validate(string json, int maxSize)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON içeriği boş olamaz.", nameof(json));
        }

        if (json.Length > maxSize)
        {
            throw new InvalidOperationException($"JSON boyutu {maxSize} karakter sınırını aşıyor.");
        }
    }
}
