namespace NLDMAP.Infrastructure.Persistence.Auditing;

public sealed class UserProfileAuditEntry
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string PreviousFirstName { get; set; } = string.Empty;
    public string PreviousLastName { get; set; } = string.Empty;
    public string NewFirstName { get; set; } = string.Empty;
    public string NewLastName { get; set; } = string.Empty;
}
