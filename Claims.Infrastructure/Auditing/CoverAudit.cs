namespace Claims.Infrastructure.Auditing;

/// <summary>An audit record for a cover API action.</summary>
public sealed class CoverAudit
{
    public int Id { get; set; }
    public string CoverId { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string HttpRequestType { get; set; } = string.Empty;
}
