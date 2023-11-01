namespace Cuplan.Organization.Models.Authentication;

public struct LoginPayload
{
    public string Email { get; set; }
    public string Password { get; set; }
}