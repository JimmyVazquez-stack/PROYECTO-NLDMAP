using Microsoft.AspNetCore.Authorization;

namespace NLDMAP.API.Authorization;

public sealed class PermissionRequirement(string permission)
    : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
