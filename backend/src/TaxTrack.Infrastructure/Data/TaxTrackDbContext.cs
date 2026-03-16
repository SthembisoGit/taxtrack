using Microsoft.EntityFrameworkCore;
using TaxTrack.Domain.Entities;

namespace TaxTrack.Infrastructure.Data;

public sealed class TaxTrackDbContext(DbContextOptions<TaxTrackDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyMembership> CompanyMemberships => Set<CompanyMembership>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<VatSubmissionRecord> VatSubmissionRecords => Set<VatSubmissionRecord>();
    public DbSet<UploadJob> UploadJobs => Set<UploadJob>();
    public DbSet<RiskAnalysisJob> RiskAnalysisJobs => Set<RiskAnalysisJob>();
    public DbSet<TaxRiskResult> TaxRiskResults => Set<TaxRiskResult>();
    public DbSet<RiskAlert> RiskAlerts => Set<RiskAlert>();
    public DbSet<DataSubjectRequest> DataSubjectRequests => Set<DataSubjectRequest>();
    public DbSet<AuditLogEvent> AuditLogEvents => Set<AuditLogEvent>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(x => x.RegistrationNumber).IsUnique();
        });

        modelBuilder.Entity<CompanyMembership>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.SourceRecordId }).IsUnique();
        });

        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.SourceRecordId }).IsUnique();
        });

        modelBuilder.Entity<VatSubmissionRecord>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.SourceRecordId }).IsUnique();
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Endpoint, x.IdempotencyKey }).IsUnique();
        });

        modelBuilder.Entity<RiskAlert>(entity =>
        {
            entity.HasOne(x => x.RiskResult)
                .WithMany(x => x.Alerts)
                .HasForeignKey(x => x.RiskResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
