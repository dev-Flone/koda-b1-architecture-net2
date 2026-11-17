namespace KodaStore.Application.DTOs.Auth;

public sealed record class AuthUserResponse(Guid Id, string Email, string DisplayName);