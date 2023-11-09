namespace Cuplan.Organization.ServiceModels.Auth0;

public struct LoginErrorResponse
{
    public string error { get; set; }
    public string error_description { get; set; }
}