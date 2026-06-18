namespace QRCodeManager.Application.Constants;

/// <summary>
/// Uygulama genelinde kullanılan sabit değerler.
/// Installer, veri yolu ve publish yapılandırması bu değerlere göre hizalanır.
/// </summary>
public static class ApplicationConstants
{
    public const string ApplicationName = "QR Manager";
    public const string ApplicationVersion = "1.0.0";
    public const string PublisherName = "Yunus Emre Teke";
    public const string DatabaseFileName = "database.db";
    public const string DatabaseDirectoryName = "database";
    public const string InitialDatabaseFileName = "InitialDatabase.db";
    public const string LogsDirectoryName = "logs";
    public const string LogFileName = "application.log";
    public const string SettingsFileName = "settings.json";
    public const string QrImagesDirectoryName = "qr-images";
}
