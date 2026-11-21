namespace KodaStore.Application.DTOs.Auth;

public record class LoginResponse(string Message, AuthUserResponse User, string AccessToken, string TokenType, DateTimeOffset ExpiresAtUtc);
