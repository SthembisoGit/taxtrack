namespace TaxTrack.Api.Common;

public sealed record HealthCheckEntryResponse(
    string Name,
    string Status,
    string? Detail);

public sealed record HealthStatusResponse(
    string Service,
    string Status,
    DateTime CheckedAtUtc,
    IReadOnlyCollection<HealthCheckEntryResponse> Checks);
