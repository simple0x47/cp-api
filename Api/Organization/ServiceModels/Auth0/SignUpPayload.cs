namespace Cuplan.Organization.ServiceModels.Auth0;

public struct SignUpPayload
{
    public string client_id { get; set; }
    public string email { get; set; }
    public string password { get; set; }
    public string connection { get; set; }
}