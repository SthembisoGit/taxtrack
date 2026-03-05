using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, UserRole role, DateTime expiresAtUtc);

    string GenerateRefreshToken();

    string HashToken(string rawToken);
}
