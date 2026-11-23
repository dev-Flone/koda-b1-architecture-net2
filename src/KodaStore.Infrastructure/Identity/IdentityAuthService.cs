using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Security;
using KodaStore.Application.Services;
using KodaStore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Identity;

public class IdentityAuthService(UserManager<AppUser> user, AppDbContext context) : IAuthService
{
    private readonly UserManager<AppUser> _user = user;
    private readonly AppDbContext _context = context;

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var email = request.Email.Trim();
        var user = await _user.FindByEmailAsync(email);

        if (user is null) return new LoginResult(false, null);

        var passwordValid = await _user.CheckPasswordAsync(user, request.Password);

        if (!passwordValid) return new LoginResult(false, null);

        var roles = await _user.GetRolesAsync(user);

        var response = new AuthUserResponse(user.Id, user.Email!, user.DisplayName, [.. roles]);

        return new LoginResult(true, response);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
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

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var createResult = await _user.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);

                var errors = createResult.Errors.ToArray();

                var duplicate = errors.Any(err => err.Code is "DuplicateEmail" or "DuplicateUsername");

                if (duplicate) return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already registered"]);

                return new RegisterResult(RegisterStatus.ValidationError, null, [.. errors.Select(err => err.Description)]);
            }

            var roleResult = await _user.AddToRoleAsync(user, AppRoles.Customer);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(err => err.Description));

                throw new InvalidOperationException($"Fail add CUSTOMER role: {errors}");
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlError && sqlError.Number is 2601 or 2627)
        {
            return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already registered"]);
        }

        // if (!identityResult.Succeeded)
        // {
        //     var error = identityResult.Errors.ToArray();

        //     var isDuplicate = error.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");

        //     if (isDuplicate) return new RegisterResult(RegisterStatus.DuplicateEmail, null, ["Email is already Registere."]);

        //     return new RegisterResult(RegisterStatus.ValidationError, null, [.. error.Select(error => error.Description)]);
        // }

        var roles = await _user.GetRolesAsync(user);

        var response = new AuthUserResponse(user.Id, user.Email, user.DisplayName, [.. roles]);

        return new RegisterResult(RegisterStatus.Created, response, []);
    }
}
