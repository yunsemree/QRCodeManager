using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.Infrastructure.Data;

/// <summary>
/// İlk çalıştırmada veritabanını hazırlar; güncellemelerde migration uygular.
/// </summary>
public sealed class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly IDatabasePathProvider _pathProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        IDatabasePathProvider pathProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseInitializationService> logger)
    {
        _pathProvider = pathProvider;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _pathProvider.EnsureApplicationDataDirectory();
            _pathProvider.CopyInitialDatabaseIfNotExists();

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pendingMigrations = await dbContext.Database
                .GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                _logger.LogInformation(
                    "Bekleyen migration uygulanıyor: {Migrations}",
                    string.Join(", ", pendingMigrations));

                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Veritabanı güncel, migration gerekmiyor.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabanı başlatılamadı.");
            throw;
        }
    }
}
