namespace NLDMAP.Infrastructure.Persistence.Auditing;

public sealed class UserAccountStatusAuditEntry
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public bool PreviousIsActive { get; set; }
    public bool NewIsActive { get; set; }
    public string Reason { get; set; } = string.Empty;
}
