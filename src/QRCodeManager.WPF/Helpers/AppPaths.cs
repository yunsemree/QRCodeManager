using QRCodeManager.Application.Constants;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.WPF.Helpers;

/// <summary>
/// WPF katmanında kullanılan statik yol yardımcıları.
/// Üretim ortamında kullanıcı verileri LocalApplicationData altında tutulur.
/// </summary>
public static class AppPaths
{
    private static readonly IDatabasePathProvider PathProvider = new Infrastructure.Data.DatabasePathProvider();

    public static string AppDataDirectory => PathProvider.ApplicationDataDirectory;
    public static string DatabasePath => PathProvider.DatabasePath;
    public static string LogFilePath => PathProvider.LogFilePath;
    public static string QrImagesDirectory => PathProvider.QrImagesDirectory;

    public static string ApplicationName => ApplicationConstants.ApplicationName;
}
