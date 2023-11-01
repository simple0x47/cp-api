namespace Cuplan.Organization.Models.Authentication;

public struct SignUpPayload
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}