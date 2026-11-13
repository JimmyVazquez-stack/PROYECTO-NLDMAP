using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Administration;

public sealed record ActorSession(string UserId, string SecurityStamp);
public interface ICurrentActor { Task<ActorSession> GetAsync(); }
public sealed class AdministrationException(string message) : Exception(message);
public sealed record AdminAccess(string UserId, string Name, bool IsAdministrator, string[] Permissions)
{
    public bool Allows(string permission) => IsAdministrator || Permissions.Contains(permission);
}
public sealed record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Total / PageSize));
}
public sealed record UserQuery(string? Search = null, bool? IsActive = null, string? RoleId = null, int Page = 1);
public sealed record UserRow(string Id, string Email, string DisplayName, bool IsActive, string[] Roles);
public sealed record UserDetail(string Id, string Email, string DisplayName, string? PhoneNumber,
    bool IsActive, string ConcurrencyStamp, string[] RoleIds);
public sealed record RoleRow(string Id, string Name, string ConcurrencyStamp, int Users, bool IsProtected, string[] Permissions);
public sealed record AuditQuery(string? Actor = null, string? Operation = null, string? Target = null,
    DateTimeOffset? From = null, DateTimeOffset? ToExclusive = null, int Page = 1);
public sealed record AuditRow(long Id, DateTimeOffset Date, string Actor, string Operation,
    string TargetType, string TargetId, string TargetName, string BeforeJson, string AfterJson);
public sealed record InvitationRow(Guid Id, string Email, string DisplayName, DateTimeOffset ExpiresAtUtc,
    string Status, string DeliveryStatus);

public sealed class UserProfileInput
{
    public string UserId { get; set; } = "";
    public string ConcurrencyStamp { get; set; } = "";
    [Required(ErrorMessage = "Escribe el nombre."), StringLength(120)]
    public string DisplayName { get; set; } = "";
    [Phone(ErrorMessage = "Escribe un teléfono válido."), StringLength(30)]
    public string? PhoneNumber { get; set; }
}
public sealed record ChangeRolesInput(string UserId, string ConcurrencyStamp, string[] RoleIds);
public sealed record ChangeStatusInput(string UserId, string ConcurrencyStamp, bool IsActive);
public sealed class RoleInput
{
    public string? Id { get; set; }
    public string? ConcurrencyStamp { get; set; }
    [Required(ErrorMessage = "Escribe el nombre del rol."), StringLength(80)]
    public string Name { get; set; } = "";
    public string[] Permissions { get; set; } = [];
}
public sealed class InvitationInput
{
    [Required, EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256)]
    public string Email { get; set; } = "";
    [Required, StringLength(120)] public string DisplayName { get; set; } = "";
    [Range(1, 7)] public int ValidDays { get; set; } = 2;
    public string[] RoleIds { get; set; } = [];
}
