namespace KodaStore.Application.DTOs.Auth;

public enum RegisterStatus
{
    Created, DuplicateEmail, ValidationError
}
public sealed record class RegisterResult(RegisterStatus Status, AuthUserResponse? User, string[] Error);
