namespace Cuplan.Organization.Models;

public class Member
{
    public Member()
    {
    }

    public Member(string id, string orgId, string userId, IEnumerable<string> permissions,
        IEnumerable<Role> roles)
    {
        Id = id;
        OrgId = orgId;
        OrgName = null;
        UserId = userId;
        Permissions = permissions;
        Roles = roles;
    }


    public Member(string id, PartialMember partialMember)
    {
        Id = id;
        OrgId = partialMember.OrgId;
        OrgName = null;
        UserId = partialMember.UserId;
        Permissions = partialMember.Permissions;
        Roles = partialMember.Roles;
    }

    public string Id { get; set; }
    public string OrgId { get; set; }
    public string? OrgName { get; set; }
    public string UserId { get; set; }
    public IEnumerable<string> Permissions { get; set; }
    public IEnumerable<Role> Roles { get; set; }
}