using QRCodeManager.Application.DTOs;

namespace QRCodeManager.Application.Interfaces;

public interface ISettingsService
{
    AppSettings GetSettings();
    Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
