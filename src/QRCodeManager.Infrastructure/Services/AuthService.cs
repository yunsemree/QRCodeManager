using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRCodeManager.Application.DTOs;
using QRCodeManager.Application.Interfaces;
using QRCodeManager.Domain.Entities;
using QRCodeManager.Infrastructure.Security;

namespace QRCodeManager.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IServiceScopeFactory scopeFactory,
        ISettingsService settingsService,
        ILogger<AuthService> logger)
    {
        _scopeFactory = scopeFactory;
        _settingsService = settingsService;
        _logger = logger;
    }

    public bool IsAuthenticated { get; private set; }

    public bool IsAuthSkipped => _settingsService.GetSettings().IsAuthSkipped;

    public UserDto? CurrentUser { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.GetSettings();
        if (settings.IsAuthSkipped || settings.CurrentUserId is null)
        {
            return;
        }

        var user = await WithUserRepository(repo => repo.GetByIdAsync(settings.CurrentUserId.Value, cancellationToken));
        if (user is null)
        {
            settings.CurrentUserId = null;
            await _settingsService.SaveSettingsAsync(settings, cancellationToken);
            return;
        }

        SetCurrentUser(user);
    }

    public async Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResultDto.Fail("E-posta ve şifre zorunludur.");
        }

        var user = await WithUserRepository(repo => repo.GetByEmailAsync(normalizedEmail, cancellationToken));
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            return AuthResultDto.Fail("E-posta veya şifre hatalı.");
        }

        await PersistSessionAsync(user, cancellationToken);
        return AuthResultDto.Ok(ToDto(user), "Giriş başarılı.");
    }

    public async Task<AuthResultDto> RegisterAsync(
        string displayName,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return AuthResultDto.Fail("Ad soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return AuthResultDto.Fail("Geçerli bir e-posta girin.");
        }

        if (password.Length < 6)
        {
            return AuthResultDto.Fail("Şifre en az 6 karakter olmalıdır.");
        }

        var existing = await WithUserRepository(repo => repo.GetByEmailAsync(normalizedEmail, cancellationToken));
        if (existing is not null)
        {
            return AuthResultDto.Fail("Bu e-posta ile kayıtlı bir hesap zaten var.");
        }

        var user = new User
        {
            DisplayName = displayName.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };

        user = await WithUserRepository(repo => repo.AddAsync(user, cancellationToken));
        await PersistSessionAsync(user, cancellationToken);
        return AuthResultDto.Ok(ToDto(user), "Kayıt başarılı.");
    }

    public async Task SkipAsync(CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.GetSettings();
        settings.IsAuthSkipped = true;
        settings.CurrentUserId = null;
        await _settingsService.SaveSettingsAsync(settings, cancellationToken);
        CurrentUser = null;
        IsAuthenticated = false;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.GetSettings();
        settings.CurrentUserId = null;
        await _settingsService.SaveSettingsAsync(settings, cancellationToken);
        CurrentUser = null;
        IsAuthenticated = false;
    }

    private async Task PersistSessionAsync(User user, CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings();
        settings.IsAuthSkipped = false;
        settings.CurrentUserId = user.Id;
        await _settingsService.SaveSettingsAsync(settings, cancellationToken);
        SetCurrentUser(user);
    }

    private void SetCurrentUser(User user)
    {
        CurrentUser = ToDto(user);
        IsAuthenticated = true;
    }

    private async Task<T> WithUserRepository<T>(Func<IUserRepository, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return await action(repository);
    }

    private static UserDto ToDto(User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName
        };

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
