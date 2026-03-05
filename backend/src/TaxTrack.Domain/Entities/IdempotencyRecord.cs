namespace TaxTrack.Domain.Entities;

public sealed class IdempotencyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Endpoint { get; set; }
    public required string IdempotencyKey { get; set; }
    public required string RequestHash { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
