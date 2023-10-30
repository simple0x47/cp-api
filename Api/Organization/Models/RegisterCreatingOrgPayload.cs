namespace Cuplan.Organization.Models;

public struct RegisterCreatingOrgPayload
{
    public string UserId { get; set; }
    public PartialOrganization Org { get; set; }
}