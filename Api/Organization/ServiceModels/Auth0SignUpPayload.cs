namespace Cuplan.Organization.ServiceModels;

public struct Auth0SignUpPayload
{
    public string client_id { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string connection { get; set; }
}