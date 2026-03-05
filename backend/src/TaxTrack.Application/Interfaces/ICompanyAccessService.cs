namespace TaxTrack.Application.Interfaces;

public interface ICompanyAccessService
{
    Task<bool> CanAccessCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken);
}
