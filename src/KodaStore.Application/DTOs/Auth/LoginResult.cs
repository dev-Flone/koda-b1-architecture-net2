namespace KodaStore.Application.DTOs.Auth;

public record class LoginResult(bool Succeeded, AuthUserResponse? User);