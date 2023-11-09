namespace Cuplan.Organization.ServiceModels.Auth0;

public struct RefreshTokenPayload
{
    public string grant_type { get; set; }
    public string client_id { get; set; }
    public string client_secret { get; set; }
    public string refresh_token { get; set; }
}