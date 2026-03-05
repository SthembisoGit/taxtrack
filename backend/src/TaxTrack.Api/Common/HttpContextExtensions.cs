using System.Security.Claims;

namespace TaxTrack.Api.Common;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("uid")?.Value ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User identity is missing.");
    }

    public static string GetCorrelationId(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.ToString();
        }

        return context.TraceIdentifier;
    }
}
