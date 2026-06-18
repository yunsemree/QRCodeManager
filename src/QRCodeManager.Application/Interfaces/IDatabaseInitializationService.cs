namespace QRCodeManager.Application.Interfaces;

/// <summary>
/// Uygulama başlangıcında SQLite veritabanını hazırlar ve EF Core migration'larını uygular.
/// </summary>
public interface IDatabaseInitializationService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
