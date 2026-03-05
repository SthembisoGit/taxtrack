using System.ComponentModel.DataAnnotations;
using TaxTrack.Domain.Common;

namespace TaxTrack.Api.Contracts;

public sealed class CreateDataRequestApiRequest
{
    public Guid? CompanyId { get; init; }

    [Required]
    public DataSubjectRequestType RequestType { get; init; }

    [MaxLength(300)]
    public string? Reason { get; init; }
}
