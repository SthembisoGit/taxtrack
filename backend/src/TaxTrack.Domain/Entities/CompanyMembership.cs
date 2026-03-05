using TaxTrack.Domain.Common;

namespace TaxTrack.Domain.Entities;

public sealed class CompanyMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public MembershipRole Role { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
    public User? User { get; set; }
}
