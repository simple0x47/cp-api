using Microsoft.AspNetCore.Authorization;

namespace Cuplan.Authorization;

public class ActiveOrganizationPermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "ActiveOrganizationPermission";

    public ActiveOrganizationPermissionAuthorizeAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission
    {
        get => Policy.Substring(PolicyPrefix.Length);
        set => Policy = $"{PolicyPrefix}{value}";
    }
}