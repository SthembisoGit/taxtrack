using TaxTrack.Domain.Common;

namespace TaxTrack.Application.Models;

public sealed record RegisterRequest(string Email, string Password, UserRole Role);

public sealed record LoginRequest(string Email, string Password);

public sealed record UserResponse(Guid Id, string Email, UserRole Role, DateTime CreatedAtUtc);

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
