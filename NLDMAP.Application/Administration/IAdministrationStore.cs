namespace NLDMAP.Application.Administration;

// Cada operación comprueba la sesión y los permisos contra los datos actuales.
// Las escrituras y sus registros de auditoría comparten una transacción.
public interface IAdministrationStore
{
    Task<AdminAccess> GetAccessAsync(ActorSession actor);
    Task<PageResult<UserRow>> GetUsersAsync(ActorSession actor, UserQuery query);
    Task<UserDetail> GetUserAsync(ActorSession actor, string id);
    Task UpdateUserAsync(ActorSession actor, UserProfileInput input);
    Task SetUserRolesAsync(ActorSession actor, ChangeRolesInput input);
    Task SetUserStatusAsync(ActorSession actor, ChangeStatusInput input);
    Task<IReadOnlyList<RoleRow>> GetRolesAsync(ActorSession actor);
    Task SaveRoleAsync(ActorSession actor, RoleInput input);
    Task DeleteRoleAsync(ActorSession actor, string id, string stamp);
    Task<PageResult<AuditRow>> GetAuditAsync(ActorSession actor, AuditQuery query);
    Task<IReadOnlyList<InvitationRow>> GetInvitationsAsync(ActorSession actor);
    Task<string> InviteAsync(ActorSession actor, InvitationInput input);
    Task<string> ResendInvitationAsync(ActorSession actor, Guid id);
    Task RevokeInvitationAsync(ActorSession actor, Guid id);
}
