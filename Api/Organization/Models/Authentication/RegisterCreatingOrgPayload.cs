namespace Cuplan.Organization.Models.Authentication;

public struct RegisterCreatingOrgPayload
{
    public SignUpPayload User { get; set; }
    public PartialOrganization Org { get; set; }
}