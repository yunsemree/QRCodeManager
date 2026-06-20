namespace QRCodeManager.Application.DTOs;

public class AuthResultDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public UserDto? User { get; init; }

    public static AuthResultDto Ok(UserDto user, string message = "İşlem başarılı.") =>
        new() { Success = true, Message = message, User = user };

    public static AuthResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}
