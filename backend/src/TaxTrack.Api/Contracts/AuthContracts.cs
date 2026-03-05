using System.ComponentModel.DataAnnotations;
using TaxTrack.Domain.Common;

namespace TaxTrack.Api.Contracts;

public sealed class RegisterApiRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public required string Password { get; init; }

    [Required]
    public UserRole Role { get; init; }
}

public sealed class LoginApiRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

public sealed class RefreshApiRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
