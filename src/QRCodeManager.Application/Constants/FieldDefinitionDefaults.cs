using QRCodeManager.Application.DTOs;

namespace QRCodeManager.Application.Constants;

public static class FieldDefinitionDefaults
{
    public static List<FieldDefinitionDto> Create() =>
    [
        new() { Key = "urun", Label = "Ürün", IsRequired = true, SortOrder = 0 },
        new() { Key = "materyal", Label = "Materyal", IsRequired = false, SortOrder = 1 },
        new() { Key = "sahibi", Label = "Sahibi", IsRequired = false, SortOrder = 2 },
        new() { Key = "konumu", Label = "Konumu", IsRequired = false, SortOrder = 3 },
        new() { Key = "seriNo", Label = "Seri No", IsRequired = true, SortOrder = 4 }
    ];
}
