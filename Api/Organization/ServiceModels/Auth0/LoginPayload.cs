namespace Cuplan.Organization.ServiceModels.Auth0;

public struct LoginPayload
{
    public string grant_type { get; set; }
    public string client_id { get; set; }
    public string client_secret { get; set; }
    public string audience { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public string scope { get; set; }
}