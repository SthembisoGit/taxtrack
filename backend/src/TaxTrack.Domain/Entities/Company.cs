namespace TaxTrack.Domain.Entities;

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Industry { get; set; }
    public required string TaxReference { get; set; }
    public Guid OwnerUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CompanyMembership> Memberships { get; set; } = new List<CompanyMembership>();
}
