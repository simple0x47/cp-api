using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Cuplan.Authorization;

public class ActiveOrganizationPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(ActiveOrganizationPermissionAuthorizeAttribute.PolicyPrefix))
        {
            string permission =
                policyName.Substring(ActiveOrganizationPermissionAuthorizeAttribute.PolicyPrefix.Length);

            AuthorizationPolicyBuilder policy = new(CookieAuthenticationDefaults.AuthenticationScheme);
            policy.AddRequirements(new ActiveOrganizationPermissionRequirement(permission));
            return policy.Build();
        }

        return null;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        AuthorizationPolicyBuilder policy = new(CookieAuthenticationDefaults.AuthenticationScheme);

        return Task.FromResult(policy.Build());
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return Task.FromResult<AuthorizationPolicy>(null);
    }
}