using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Identity;

public class IdentityAuthService(UserManager<AppUser> user) : IAuthService
{
    private readonly UserManager<AppUser> _user = user;
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var email = request.Email.Trim();
        var user = await _user.FindByEmailAsync(email);

        if (user is null) return new LoginResult(false, null);

        var passwordValid = await _user.CheckPasswordAsync(user, request.Password);

        if (!passwordValid) return new LoginResult(false, null);

        var response = new AuthUserResponse(user.Id, user.Email!, user.DisplayName);

        return new LoginResult(true, response);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var email = request.Email.Trim();
        var emailExists = await _user.FindByEmailAsync(email);
        if (emailExists is not null) return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already registered"]);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        IdentityResult identityResult;

        try
        {
            identityResult = await _user.CreateAsync(user, request.Password);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlError)
        {
            return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already registered"]);
        }

        if (!identityResult.Succeeded)
        {
            var error = identityResult.Errors.ToArray();

            var isDuplicate = error.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");

            if (isDuplicate) return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already Registere."]);

            return new RegisterResult(RegisterStatus.ValidationError, null, [.. error.Select(error => error.Description)]);
        }

        var response = new AuthUserResponse(user.Id, user.Email, user.DisplayName);

        return new RegisterResult(RegisterStatus.Created, response, []);
    }
}
