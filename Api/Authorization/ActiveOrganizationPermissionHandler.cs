using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Cuplan.Authorization;

public class ActiveOrganizationPermissionHandler : AuthorizationHandler<ActiveOrganizationPermissionRequirement>
{
    private const string ActiveOrganizationHeader = "ActiveOrganization";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ActiveOrganizationPermissionRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
        {
            context.Fail(new AuthorizationFailureReason(this, "'context.Resource' is not a 'HttpContext'."));
            return Task.CompletedTask;
        }

        if (!httpContext.Request.Headers.ContainsKey(ActiveOrganizationHeader))
        {
            context.Fail(new AuthorizationFailureReason(this,
                $"Request does not contain '{ActiveOrganizationHeader}' header."));
            return Task.CompletedTask;
        }

        string? activeOrg = httpContext.Request.Headers[ActiveOrganizationHeader];

        if (activeOrg is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "Request does not contain an active organization."));
            return Task.CompletedTask;
        }

        foreach (Claim claim in context.User.Claims)
            if (claim.Type.Equals(activeOrg))
            {
                MembershipPermissions? membership =
                    JsonConvert.DeserializeObject<MembershipPermissions>(claim.Value);

                if (membership is null)
                {
                    context.Fail(new AuthorizationFailureReason(this,
                        $"Membership to organization '{claim.Type}' is null."));
                    return Task.CompletedTask;
                }

                if (membership.Value.Permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                break;
            }

        context.Fail(new AuthorizationFailureReason(this,
            $"Membership misses the required permission '{requirement.Permission}'."));
        return Task.CompletedTask;
    }
}