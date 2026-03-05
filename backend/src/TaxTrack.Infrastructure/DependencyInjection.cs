using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaxTrack.Application.Interfaces;
using TaxTrack.Infrastructure.Data;
using TaxTrack.Infrastructure.Options;
using TaxTrack.Infrastructure.Services;

namespace TaxTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<TaxPolicyOptions>(configuration.GetSection(TaxPolicyOptions.SectionName));

        var connectionString = configuration.GetConnectionString("PostgreSql");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<TaxTrackDbContext>(opt => opt.UseInMemoryDatabase("taxtrack-dev"));
        }
        else
        {
            services.AddDbContext<TaxTrackDbContext>(opt => opt.UseNpgsql(connectionString));
        }

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ICompanyAccessService, CompanyAccessService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUploadService, UploadService>();
        services.AddScoped<IRiskService, RiskService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPrivacyService, PrivacyService>();

        return services;
    }
}
