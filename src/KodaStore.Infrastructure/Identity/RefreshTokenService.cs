
using System.Security.Cryptography;
using System.Text;
using KodaStore.Application.DTOs.Auth;
using KodaStore.Application.Services;
using KodaStore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KodaStore.Infrastructure.Identity;

public sealed class RefreshTokenService(AppDbContext context, UserManager<AppUser> userManager, IJwtTokenService jwtTokenService) : IRefreshTokenService
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);
    public async Task<RefreshTokenSessionResult> CreateSessionAsync(AuthUserResponse user, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var rawToken = GenerateToken();
        var expiredAt = now.Add(RefreshLifetime);

        var accessToken = jwtTokenService.IssueAccessToken(user);
        var tokenEntity = CreateTokenEntity(user.Id, Guid.NewGuid(), HashToken(rawToken), now, expiredAt);

        context.RefreshToken.Add(tokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenSessionResult(user, accessToken, rawToken, expiredAt);
    }

    public async Task RevokeFamilyAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(rawRefreshToken);
        var token = await context.RefreshToken.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (token is not null) await RevokeFamilyByIdAsync(token.FamilyId, cancellationToken);
    }

    private Task RevokeFamilyByIdAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return context.RefreshToken.Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null).ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, (DateTimeOffset?)now), cancellationToken);
    }

    public async Task<RefreshTokenSessionResult> RotateAsync(string RawRefreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(RawRefreshToken);
        var now = DateTimeOffset.UtcNow;

        var currentToken = await context.RefreshToken.AsNoTracking().SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (currentToken is null) return null!;
        if (currentToken.UsedAtUtc is not null)
        {
            await RevokeFamilyByIdAsync(currentToken.FamilyId, cancellationToken);
            return null!;
        }

        if (currentToken.RevokedAtUtc is not null || currentToken.ExpiresAtUtc <= now)
        {
            return null!;
        }

        var identityUser = await userManager.FindByIdAsync(currentToken.UserId.ToString("D"));
        if (identityUser is null || string.IsNullOrWhiteSpace(identityUser.Email)) return null!;

        var user = new AuthUserResponse(identityUser.Id, identityUser.Email, identityUser.DisplayName);
        var nextRawToken = GenerateToken();
        var nextExpiresAt = now.Add(RefreshLifetime);
        var nextAccessToken = jwtTokenService.IssueAccessToken(user);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var affectedRows = await context.RefreshToken
            .Where(t =>
                t.Id == currentToken.Id &&
                t.UsedAtUtc == null &&
                t.RevokedAtUtc == null &&
                t.ExpiresAtUtc > now
            ).ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAtUtc, (DateTimeOffset?)now), cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null!;
        }

        var newTokenEntity = CreateTokenEntity(
            currentToken.UserId,
            currentToken.FamilyId,
            HashToken(nextRawToken),
            now,
            nextExpiresAt
        );

        context.RefreshToken.Add(newTokenEntity);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RefreshTokenSessionResult(user, nextAccessToken, nextRawToken, nextExpiresAt);
    }

    private static RefreshToken CreateTokenEntity(Guid userId, Guid familyId, string tokenHash, DateTimeOffset now, DateTimeOffset expireAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        FamilyId = familyId,
        TokenHash = tokenHash,
        CreatedAtUtc = now,
        ExpiresAtUtc = expireAt
    };

    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string rawToken) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
