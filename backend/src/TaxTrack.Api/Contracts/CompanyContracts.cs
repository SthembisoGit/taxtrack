using System.ComponentModel.DataAnnotations;

namespace TaxTrack.Api.Contracts;

public sealed class CreateCompanyApiRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public required string Name { get; init; }

    [Required]
    [MinLength(5)]
    [MaxLength(30)]
    public required string RegistrationNumber { get; init; }

    [Required]
    public required string Industry { get; init; }

    [Required]
    [MinLength(10)]
    [MaxLength(15)]
    public required string TaxReference { get; init; }
}
