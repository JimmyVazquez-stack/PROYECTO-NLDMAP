using Microsoft.AspNetCore.Authorization;

namespace NLDMAP.API.Authorization;

/// <summary>
/// Domain operations must validate ownership, assignment, authorized scope,
/// workflow state, and any applicable profile verification.
/// The domain module must supply a resource-aware handler for this requirement.
/// Until that handler exists, functional policies do not authorize operations.
/// </summary>
public sealed class PermissionScopeRequirement(string permission)
    : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
