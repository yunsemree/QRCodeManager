using System.Text.Json;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.Constants;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;

namespace QRCodeManager.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsFilePath;
    private readonly ILogger<SettingsService> _logger;
    private AppSettings _cachedSettings;

    public SettingsService(IDatabasePathProvider pathProvider, ILogger<SettingsService> logger)
    {
        _logger = logger;
        pathProvider.EnsureApplicationDataDirectory();
        _settingsFilePath = pathProvider.SettingsFilePath;
        _cachedSettings = LoadFromDisk();
    }

    public AppSettings GetSettings() => _cachedSettings;

    public async Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken);
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar kaydedilemedi.");
            throw;
        }
    }

    private AppSettings LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            if (settings.FieldDefinitions is null || settings.FieldDefinitions.Count == 0)
            {
                settings.FieldDefinitions = FieldDefinitionDefaults.Create();
            }

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ayarlar okunamadı, varsayılanlar kullanılacak.");
            return new AppSettings();
        }
    }
}
