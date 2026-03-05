using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaxTrack.Api.Common;
using TaxTrack.Api.Contracts;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;

namespace TaxTrack.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterApiRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(
            new RegisterRequest(request.Email, request.Password, request.Role),
            cancellationToken);

        return Created("/api/auth/register", response);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginApiRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(
            new LoginRequest(request.Email, request.Password),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            HttpContext.GetCorrelationId(),
            cancellationToken);

        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshApiRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RefreshAsync(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            HttpContext.GetCorrelationId(),
            cancellationToken);

        return response is null ? Unauthorized() : Ok(response);
    }
}
