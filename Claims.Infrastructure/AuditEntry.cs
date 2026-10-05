namespace Claims.Infrastructure;

public sealed class AuditEntry
{
    public long Id { get; set; }
    public string Entity { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Operation { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}
