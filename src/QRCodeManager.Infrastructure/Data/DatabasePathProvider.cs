using QRCodeManager.Application.Constants;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.Infrastructure.Data;

/// <summary>
/// Kurulum dizini (Program Files) salt okunurdur; yazılabilir veriler LocalApplicationData altında tutulur.
/// </summary>
public sealed class DatabasePathProvider : IDatabasePathProvider
{
    public string ApplicationDataDirectory { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationConstants.ApplicationName);

    public string DatabaseDirectory =>
        Path.Combine(ApplicationDataDirectory, ApplicationConstants.DatabaseDirectoryName);

    public string DatabasePath =>
        Path.Combine(DatabaseDirectory, ApplicationConstants.DatabaseFileName);

    public string LogFilePath =>
        Path.Combine(ApplicationDataDirectory, ApplicationConstants.LogsDirectoryName, ApplicationConstants.LogFileName);

    public string SettingsFilePath =>
        Path.Combine(ApplicationDataDirectory, ApplicationConstants.SettingsFileName);

    public string QrImagesDirectory =>
        Path.Combine(ApplicationDataDirectory, ApplicationConstants.QrImagesDirectoryName);

    /// <summary>Kurulum dizinindeki şablon veritabanı (salt okunur).</summary>
    public string BundledInitialDatabasePath =>
        Path.Combine(AppContext.BaseDirectory, ApplicationConstants.InitialDatabaseFileName);

    public void EnsureApplicationDataDirectory()
    {
        Directory.CreateDirectory(ApplicationDataDirectory);
        Directory.CreateDirectory(DatabaseDirectory);
        Directory.CreateDirectory(QrImagesDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
    }

    public void CopyInitialDatabaseIfNotExists()
    {
        EnsureApplicationDataDirectory();

        if (File.Exists(DatabasePath))
        {
            return;
        }

        if (!File.Exists(BundledInitialDatabasePath))
        {
            throw new FileNotFoundException(
                "Başlangıç veritabanı dosyası bulunamadı.",
                BundledInitialDatabasePath);
        }

        File.Copy(BundledInitialDatabasePath, DatabasePath);
    }
}
