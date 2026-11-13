namespace NLDMAP.Domain.Accounts;

public sealed class AccountInvitation
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string NormalizedEmail { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string TokenHash { get; set; } = "";
    public string RoleIdsJson { get; set; } = "[]";
    public string CreatedById { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? AcceptedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string DeliveryStatus { get; set; } = "Pendiente";
}
