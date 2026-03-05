using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        CancellationToken cancellationToken);

    Task<AuthResponse?> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        string correlationId,
        CancellationToken cancellationToken);
}
