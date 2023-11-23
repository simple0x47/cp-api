namespace Cuplan.Authentication.Models;

public struct LoginSuccessPayload
{
    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public ulong ExpiresIn { get; set; }
}