
using KodaStore.Application.DTOs.Auth;

namespace KodaStore.Application.Services;

public interface IRefreshTokenService
{
    Task<RefreshTokenSessionResult> CreateSessionAsync(AuthUserResponse user, CancellationToken cancellationToken = default);
    Task<RefreshTokenSessionResult> RotateAsync(string RawRefreshToken, CancellationToken cancellationToken = default);
    Task RevokeFamilyAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
}
