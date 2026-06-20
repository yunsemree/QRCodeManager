using QRCodeManager.Application.DTOs;

namespace QRCodeManager.Application.Interfaces;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    bool IsAuthSkipped { get; }
    UserDto? CurrentUser { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RegisterAsync(string displayName, string email, string password, CancellationToken cancellationToken = default);
    Task SkipAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
