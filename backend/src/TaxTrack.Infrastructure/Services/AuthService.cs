using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;

namespace TaxTrack.Infrastructure.Services;

public sealed class AuthService(
    TaxTrackDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IAuditService auditService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id, user.Email, user.Role, user.CreatedAtUtc);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            if (user is not null)
            {
                await auditService.LogAsync(
                    user.Id,
                    null,
                    Domain.Common.AuditEventType.LoginFailed,
                    correlationId,
                    new { request.Email },
                    ipAddress,
                    userAgent,
                    cancellationToken);
            }

            return null;
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        var accessExpires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.Role, accessExpires);
        var refreshTokenRaw = jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = jwtTokenService.HashToken(refreshTokenRaw);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = refreshExpires
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            user.Id,
            null,
            Domain.Common.AuditEventType.LoginSucceeded,
            correlationId,
            new { user.Email },
            ipAddress,
            userAgent,
            cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.Role,
            accessToken,
            refreshTokenRaw,
            accessExpires,
            refreshExpires);
    }

    public async Task<AuthResponse?> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var refreshTokenHash = jwtTokenService.HashToken(refreshToken);
        var existingToken = await dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == refreshTokenHash &&
                     x.RevokedAtUtc == null,
                cancellationToken);

        if (existingToken is null || existingToken.User is null || existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        // One-time refresh token rotation: revoke current token before issuing a replacement.
        existingToken.RevokedAtUtc = DateTime.UtcNow;

        var user = existingToken.User;
        var accessExpires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshExpires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Email, user.Role, accessExpires);
        var refreshTokenRaw = jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = jwtTokenService.HashToken(refreshTokenRaw);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = refreshExpires
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            user.Id,
            null,
            Domain.Common.AuditEventType.TokenRefreshed,
            correlationId,
            new { user.Email },
            ipAddress,
            userAgent,
            cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.Role,
            accessToken,
            refreshTokenRaw,
            accessExpires,
            refreshExpires);
    }
}
