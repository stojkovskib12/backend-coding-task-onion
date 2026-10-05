namespace Claims.Application.Abstractions;

public sealed record AuditMessage(string Entity, string EntityId, string Operation, DateTimeOffset OccurredAt);
