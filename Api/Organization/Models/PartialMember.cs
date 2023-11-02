namespace Cuplan.Organization.Models;

public class PartialMember
{
    public PartialMember(string orgId, string userId, IEnumerable<string> permissions, IEnumerable<Role> roles)
    {
        OrgId = orgId;
        UserId = userId;
        Permissions = permissions;
        Roles = roles;
    }

    public string OrgId { get; set; }
    public string UserId { get; set; }
    public IEnumerable<string> Permissions { get; set; }
    public IEnumerable<Role> Roles { get; set; }
}