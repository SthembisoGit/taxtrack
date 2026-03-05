using System.ComponentModel.DataAnnotations;

namespace TaxTrack.Api.Contracts;

public sealed class UploadApiRequest
{
    [Required]
    public Guid CompanyId { get; init; }

    [Required]
    public required string DatasetType { get; init; }

    [Required]
    public required IFormFile File { get; init; }
}
