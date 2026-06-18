namespace QRCodeManager.Application.Interfaces;

/// <summary>
/// Kullanıcı verisi ve SQLite veritabanı yollarını yönetir.
/// Yazılabilir veriler LocalApplicationData altında tutulur.
/// </summary>
public interface IDatabasePathProvider
{
    /// <summary>Örn: C:\Users\...\AppData\Local\QR Manager</summary>
    string ApplicationDataDirectory { get; }

    /// <summary>Örn: C:\Users\...\AppData\Local\QR Manager\database</summary>
    string DatabaseDirectory { get; }

    /// <summary>Örn: C:\Users\...\AppData\Local\QR Manager\database\database.db</summary>
    string DatabasePath { get; }

    string LogFilePath { get; }
    string SettingsFilePath { get; }
    string QrImagesDirectory { get; }

    /// <summary>Kurulum dizinindeki şablon veritabanı (InitialDatabase.db).</summary>
    string BundledInitialDatabasePath { get; }

    void EnsureApplicationDataDirectory();

    /// <summary>
    /// Veritabanı yoksa kurulum dizinindeki InitialDatabase.db dosyasını kopyalar.
    /// </summary>
    void CopyInitialDatabaseIfNotExists();
}
