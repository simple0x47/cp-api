namespace Cuplan.Organization.Models;

public struct UserCreateOrgPayload
{
    public string UserId { get; set; }
    public PartialOrganization Org { get; set; }
}