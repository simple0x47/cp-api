namespace Cuplan.Organization.Models;

public class PartialMember(string orgId, string userId, IEnumerable<string> permissions, IEnumerable<Role> roles)
{
    public string OrgId { get; set; } = orgId;
    public string UserId { get; set; } = userId;
    public IEnumerable<string> Permissions { get; set; } = permissions;
    public IEnumerable<Role> Roles { get; set; } = roles;
}