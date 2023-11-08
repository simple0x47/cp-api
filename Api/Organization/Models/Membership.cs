namespace Cuplan.Organization.Models;

public class Membership
{
    public Membership()
    {
    }

    public Membership(string id, string orgId, string userId, IEnumerable<string> permissions,
        IEnumerable<Role> roles)
    {
        Id = id;
        OrgId = orgId;
        OrgName = null;
        UserId = userId;
        Permissions = permissions;
        Roles = roles;
    }


    public Membership(string id, PartialMembership partialMembership)
    {
        Id = id;
        OrgId = partialMembership.OrgId;
        OrgName = null;
        UserId = partialMembership.UserId;
        Permissions = partialMembership.Permissions;
        Roles = partialMembership.Roles;
    }

    public string Id { get; set; }
    public string OrgId { get; set; }
    public string? OrgName { get; set; }
    public string UserId { get; set; }
    public IEnumerable<string> Permissions { get; set; }
    public IEnumerable<Role> Roles { get; set; }
}