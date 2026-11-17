using System;
using KodaStore.Application.DTOs.Auth;

namespace KodaStore.Application.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
