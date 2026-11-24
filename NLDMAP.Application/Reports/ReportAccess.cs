using NLDMAP.Application.Accounts;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Application.Reports;

public sealed class ReportAccess(IAdministrationStore administration, ICitizenAccounts accounts)
{
    public async Task<bool> CanCreateAsync(ActorSession actor)
    {
        var (administrator, citizen) = await GetCreationAccessAsync(actor);
        return administrator || citizen is not null;
    }

    internal async Task<Reportante> RequireReporterAsync(ActorSession actor)
    {
        var (administrator, citizen) = await GetCreationAccessAsync(actor);
        if (administrator)
        {
            var profile = await administration.GetUserAsync(actor, actor.UserId);
            return new(profile.DisplayName, profile.Email, profile.PhoneNumber);
        }

        if (citizen is not null)
            return new(citizen.DisplayName, citizen.Email, citizen.PhoneNumber);

        throw new AdministrationException("Tu cuenta no tiene permiso para crear reportes.");
    }

    private async Task<(bool Administrator, CitizenProfile? Citizen)> GetCreationAccessAsync(ActorSession actor)
    {
        if (await GetStaffAccessAsync(actor) is { } staff)
            return (staff.IsAdministrator, null);

        var profile = await accounts.GetProfileAsync(actor);
        return (false, profile?.CanCreateReports == true ? profile : null);
    }

    internal async Task RequireReadAsync(ActorSession actor, bool all)
    {
        if (all)
        {
            var staff = await administration.GetAccessAsync(actor);
            AdministrationRules.RequirePermission(staff, PermissionNames.ReportsRead);
            return;
        }

        if (await GetStaffAccessAsync(actor) is not null || await accounts.GetProfileAsync(actor) is not null)
            return;

        throw new AdministrationException("Inicia sesión nuevamente para consultar tus reportes.");
    }

    private async Task<AdminAccess?> GetStaffAccessAsync(ActorSession actor)
    {
        try
        {
            return await administration.GetAccessAsync(actor);
        }
        catch (AdministrationException)
        {
            // Las cuentas públicas se validan con el servicio de perfiles ciudadanos.
            return null;
        }
    }
}
