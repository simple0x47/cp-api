using Cuplan.Authentication.Models;

namespace Cuplan.Organization.ServiceModels.Auth0;

public struct LoginOkResponse
{
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string id_token { get; set; }
    public string scope { get; set; }
    public ulong expires_in { get; set; }
    public string token_type { get; set; }

    public static implicit operator LoginSuccessPayload(LoginOkResponse okResponse)
    {
        LoginSuccessPayload payload = new()
        {
            AccessToken = okResponse.access_token,
            IdToken = okResponse.id_token,
            RefreshToken = okResponse.refresh_token,
            ExpiresIn = okResponse.expires_in
        };

        return payload;
    }
}