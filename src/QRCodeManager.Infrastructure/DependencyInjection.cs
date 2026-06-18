using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Infrastructure.Data;
using QRCodeManager.Infrastructure.Logging;
using QRCodeManager.Infrastructure.Repositories;
using QRCodeManager.Infrastructure.Services;

namespace QRCodeManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDatabasePathProvider, DatabasePathProvider>();
        services.AddSingleton<IDatabaseInitializationService, DatabaseInitializationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IJsonService, JsonService>();
        services.AddSingleton<IQrService, QrService>();
        services.AddSingleton<IAssetFormService, AssetFormService>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var pathProvider = serviceProvider.GetRequiredService<IDatabasePathProvider>();
            options.UseSqlite($"Data Source={pathProvider.DatabasePath}");
        });

        services.AddScoped<IHistoryRepository, HistoryRepository>();

        services.AddSingleton<ILoggerProvider>(serviceProvider =>
            new FileLoggerProvider(serviceProvider.GetRequiredService<IDatabasePathProvider>().LogFilePath));

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var initializationService = serviceProvider.GetRequiredService<IDatabaseInitializationService>();
        await initializationService.InitializeAsync();
    }
}
