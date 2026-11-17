namespace KodaStore.Application.DTOs.Auth;

public record class LoginResponse(string Message, AuthUserResponse User);
