namespace Cuplan.Organization.Models;

public struct RegisterCreatingOrgPayload
{
    public SignUpPayload User { get; set; }
    public PartialOrganization Org { get; set; }
}