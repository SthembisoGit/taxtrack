using TaxTrack.Application.Models;

namespace TaxTrack.Application.Interfaces;

public interface ICompanyService
{
    Task<CompanyResponse> CreateCompanyAsync(Guid userId, CreateCompanyRequest request, CancellationToken cancellationToken);

    Task<CompanyResponse?> GetCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken);
}
