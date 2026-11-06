namespace NLDMAP.Domain.Auditing;

public sealed class AuditEntry
{
    public long Id { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string ActorId { get; set; } = "";
    public string ActorName { get; set; } = "";
    public string Operation { get; set; } = "";
    public string TargetType { get; set; } = "";
    public string TargetId { get; set; } = "";
    public string TargetName { get; set; } = "";
    public string BeforeJson { get; set; } = "{}";
    public string AfterJson { get; set; } = "{}";
}
