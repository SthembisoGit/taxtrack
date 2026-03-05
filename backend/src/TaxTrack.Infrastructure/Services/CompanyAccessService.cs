using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Interfaces;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class CompanyAccessService(TaxTrackDbContext dbContext) : ICompanyAccessService
{
    public Task<bool> CanAccessCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken)
    {
        return dbContext.CompanyMemberships
            .AnyAsync(x => x.CompanyId == companyId && x.UserId == userId && x.IsActive, cancellationToken);
    }
}
