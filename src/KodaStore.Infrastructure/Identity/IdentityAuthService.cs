using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Services;
using Microsoft.AspNetCore.Identity;

namespace KodaStore.Infrastructure.Identity;

public class IdentityAuthService(UserManager<AppUser> user) : IAuthService
{
    private readonly UserManager<AppUser> _user = user;
    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
