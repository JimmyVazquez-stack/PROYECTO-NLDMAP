using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;

namespace NLDMAP.Application.Catalogs;

public sealed class CatalogAccess(ICurrentActor currentActor, IAdministrationStore administration)
{
    internal async Task<ActorSession> AuthorizeAsync(bool write)
    {
        var actor = await currentActor.GetAsync();
        var access = await administration.GetAccessAsync(actor);
        AdministrationRules.RequirePermission(access, PermissionNames.CatalogsRead);
        if (write)
            AdministrationRules.RequirePermission(access, PermissionNames.CatalogsManage);
        return actor;
    }
}
