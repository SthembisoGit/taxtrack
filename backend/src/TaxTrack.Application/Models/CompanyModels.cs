namespace TaxTrack.Application.Models;

public sealed record CreateCompanyRequest(string Name, string RegistrationNumber, string Industry, string TaxReference);

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string RegistrationNumber,
    string Industry,
    string TaxReference,
    Guid OwnerUserId,
    DateTime CreatedAtUtc);
