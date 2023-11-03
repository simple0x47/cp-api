using Microsoft.AspNetCore.Authorization;

namespace Cuplan.Authorization;

public class ActiveOrganizationPermissionRequirement : IAuthorizationRequirement
{
    public ActiveOrganizationPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}