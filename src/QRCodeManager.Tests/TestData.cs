using QRCodeManager.Application.Constants;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using Moq;

namespace QRCodeManager.Tests;

internal static class TestData
{
    public static ISettingsService CreateSettingsService()
    {
        var mock = new Mock<ISettingsService>();
        mock.Setup(x => x.GetSettings()).Returns(new AppSettings
        {
            MaximumJsonSize = 4096,
            FieldDefinitions = FieldDefinitionDefaults.Create()
        });
        return mock.Object;
    }

    public static AssetFormDto CreateSampleForm()
    {
        var form = new AssetFormDto();
        form.SetValue("urun", "Laptop");
        form.SetValue("materyal", "Alüminyum");
        form.SetValue("sahibi", "Yunus Emre Teke");
        form.SetValue("konumu", "Sivas Halk Eğitim Merkezi");
        form.SetValue("seriNo", "ABC123456");
        return form;
    }
}
