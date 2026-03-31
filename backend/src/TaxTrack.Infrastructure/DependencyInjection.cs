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
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<ReportDownloadOptions>(configuration.GetSection(ReportDownloadOptions.SectionName));
        services.Configure<TaxPolicyOptions>(configuration.GetSection(TaxPolicyOptions.SectionName));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString("PostgreSql");
        if (databaseOptions.UseInMemory)
        {
            if (!string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Database:UseInMemory is allowed only in Development or Testing environments.");
            }

            services.AddDbContext<TaxTrackDbContext>(opt => opt.UseInMemoryDatabase("taxtrack-dev"));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:PostgreSql must be configured when Database:UseInMemory is false.");
            }

            services.AddDbContext<TaxTrackDbContext>(opt => opt.UseNpgsql(connectionString));
        }

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
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
