namespace Cuplan.Authentication.Models;

public struct LoginPayload
{
    public string Email { get; set; }
    public string Password { get; set; }
}