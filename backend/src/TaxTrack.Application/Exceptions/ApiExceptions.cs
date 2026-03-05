using TaxTrack.Application.Models;

namespace TaxTrack.Application.Exceptions;

public sealed class ConflictException(string message) : Exception(message);

public sealed class ForbiddenException(string message) : Exception(message);

public sealed class ValidationException(string message, IReadOnlyCollection<ValidationIssue> issues) : Exception(message)
{
    public IReadOnlyCollection<ValidationIssue> Issues { get; } = issues;
}
