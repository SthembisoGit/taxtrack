using Microsoft.EntityFrameworkCore;
using TaxTrack.Application.Exceptions;
using TaxTrack.Application.Interfaces;
using TaxTrack.Application.Models;
using TaxTrack.Domain.Common;
using TaxTrack.Domain.Entities;
using TaxTrack.Infrastructure.Data;

namespace TaxTrack.Infrastructure.Services;

public sealed class CompanyService(TaxTrackDbContext dbContext) : ICompanyService
{
    public async Task<CompanyResponse> CreateCompanyAsync(Guid userId, CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Companies
            .AnyAsync(x => x.RegistrationNumber == request.RegistrationNumber, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Company registration number already exists.");
        }

        var company = new Company
        {
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Industry = request.Industry.Trim(),
            TaxReference = request.TaxReference.Trim(),
            OwnerUserId = userId
        };

        dbContext.Companies.Add(company);
        dbContext.CompanyMemberships.Add(new CompanyMembership
        {
            CompanyId = company.Id,
            UserId = userId,
            Role = MembershipRole.Owner,
            CreatedByUserId = userId
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(company);
    }

    public async Task<CompanyResponse?> GetCompanyAsync(Guid userId, Guid companyId, CancellationToken cancellationToken)
    {
        var canAccess = await dbContext.CompanyMemberships.AnyAsync(
            x => x.CompanyId == companyId && x.UserId == userId && x.IsActive,
            cancellationToken);

        if (!canAccess)
        {
            throw new ForbiddenException("User cannot access this company.");
        }

        var company = await dbContext.Companies.FirstOrDefaultAsync(x => x.Id == companyId, cancellationToken);
        return company is null ? null : ToResponse(company);
    }

    private static CompanyResponse ToResponse(Company company)
    {
        return new CompanyResponse(
            company.Id,
            company.Name,
            company.RegistrationNumber,
            company.Industry,
            company.TaxReference,
            company.OwnerUserId,
            company.CreatedAtUtc);
    }
}
