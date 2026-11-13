using System.ComponentModel.DataAnnotations;
using NLDMAP.Application.Security;

namespace NLDMAP.Application.Administration;

// La presentación solicita casos de uso sin conocer Identity ni EF Core.
public sealed class AdministrationService(ICurrentActor currentActor, IAdministrationStore store)
{
    public async Task<AdminAccess> GetAccessAsync() => await store.GetAccessAsync(await currentActor.GetAsync());
    public async Task<PageResult<UserRow>> GetUsersAsync(UserQuery q) => await store.GetUsersAsync(await AuthorizedAsync(PermissionNames.UsersRead), q);
    public async Task<UserDetail> GetUserAsync(string id) => await store.GetUserAsync(await AuthorizedAsync(PermissionNames.UsersRead), id);
    public async Task UpdateUserAsync(UserProfileInput input)
    {
        Validate(input);
        input.DisplayName = input.DisplayName.Trim();
        if (input.DisplayName.Length == 0) throw new AdministrationException("Escribe el nombre.");
        await store.UpdateUserAsync(await AuthorizedAsync(PermissionNames.UsersEdit), input);
    }
    public async Task SetUserStatusAsync(ChangeStatusInput input) => await store.SetUserStatusAsync(await AuthorizedAsync(PermissionNames.UsersStatus), input);
    public async Task SetUserRolesAsync(ChangeRolesInput input) => await store.SetUserRolesAsync(await AuthorizedAsync(PermissionNames.RolesAssign), input);
    public async Task<IReadOnlyList<RoleRow>> GetRolesAsync() => await store.GetRolesAsync(await currentActor.GetAsync());
    public async Task SaveRoleAsync(RoleInput input)
    {
        Validate(input);
        input.Name = input.Name.Trim();
        if (input.Name.Length == 0 || input.Permissions.Any(p => !PermissionNames.Catalog.ContainsKey(p)))
            throw new AdministrationException("El nombre o los permisos del rol no son válidos.");
        input.Permissions = input.Permissions.Distinct().ToArray();
        await store.SaveRoleAsync(await AuthorizedAsync(PermissionNames.RolesManage), input);
    }
    public async Task DeleteRoleAsync(string id, string stamp) => await store.DeleteRoleAsync(await AuthorizedAsync(PermissionNames.RolesManage), id, stamp);
    public async Task<PageResult<AuditRow>> GetAuditAsync(AuditQuery query) => await store.GetAuditAsync(await AuthorizedAsync(PermissionNames.AuditRead), query);
    public async Task<IReadOnlyList<InvitationRow>> GetInvitationsAsync() => await store.GetInvitationsAsync(await AuthorizedAsync(PermissionNames.InvitationsManage));
    public async Task<string> InviteAsync(InvitationInput input)
    {
        Validate(input);
        input.Email = input.Email.Trim(); input.DisplayName = input.DisplayName.Trim();
        if (input.DisplayName.Length == 0 || input.RoleIds.Length == 0)
            throw new AdministrationException("Escribe el nombre y selecciona al menos un rol.");
        return await store.InviteAsync(await AuthorizedAsync(PermissionNames.InvitationsManage), input);
    }
    public async Task<string> ResendInvitationAsync(Guid id) => await store.ResendInvitationAsync(await AuthorizedAsync(PermissionNames.InvitationsManage), id);
    public async Task RevokeInvitationAsync(Guid id) => await store.RevokeInvitationAsync(await AuthorizedAsync(PermissionNames.InvitationsManage), id);
    private async Task<ActorSession> AuthorizedAsync(string permission)
    {
        var actor = await currentActor.GetAsync();
        AdministrationRules.RequirePermission(await store.GetAccessAsync(actor), permission);
        return actor;
    }
    private static void Validate(object model)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, true))
            throw new AdministrationException(string.Join(" ", errors.Select(e => e.ErrorMessage)));
    }
}
