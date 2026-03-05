using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;
using TaxTrack.Infrastructure.Services;

namespace TaxTrack.Tests;

public sealed class AuthRefreshTests
{
    [Fact]
    public async Task Refresh_RotatesToken_AndRejectsOldToken()
    {
        await using var dbContext = CreateDbContext();
        var authService = CreateAuthService(dbContext);

        await authService.RegisterAsync(
            new RegisterRequest("owner@taxtrack.test", "StrongPass!1234", UserRole.Owner),
            CancellationToken.None);

        var login = await authService.LoginAsync(
            new LoginRequest("owner@taxtrack.test", "StrongPass!1234"),
            "127.0.0.1",
            "xunit",
            "corr-login",
            CancellationToken.None);

        Assert.NotNull(login);

        var firstRefresh = await authService.RefreshAsync(
            login!.RefreshToken,
            "127.0.0.1",
            "xunit",
            "corr-refresh-1",
            CancellationToken.None);

        Assert.NotNull(firstRefresh);
        Assert.NotEqual(login.RefreshToken, firstRefresh!.RefreshToken);

        var replayAttempt = await authService.RefreshAsync(
            login.RefreshToken,
            "127.0.0.1",
            "xunit",
            "corr-refresh-2",
            CancellationToken.None);

        Assert.Null(replayAttempt);
    }

    private static TaxTrackDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TaxTrackDbContext>()
            .UseInMemoryDatabase($"taxtrack-tests-{Guid.NewGuid()}")
            .Options;
        return new TaxTrackDbContext(options);
    }

    private static AuthService CreateAuthService(TaxTrackDbContext dbContext)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "TaxTrack.Tests",
            Audience = "TaxTrack.Tests.Client",
            SigningKey = "0123456789abcdef0123456789abcdef",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        return new AuthService(
            dbContext,
            new PasswordHasher(),
            new JwtTokenService(jwtOptions),
            new NoOpAuditService(),
            jwtOptions);
    }

    private sealed class NoOpAuditService : IAuditService
    {
        public Task LogAsync(
            Guid actorUserId,
            Guid? companyId,
            AuditEventType eventType,
            string correlationId,
            object metadata,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
