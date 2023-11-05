using System.Net;
using Core;
using Core.Secrets;
using Cuplan.Organization.Models.Authentication;
using Cuplan.Organization.ServiceModels.Auth0;
using Newtonsoft.Json;
using LoginPayload = Cuplan.Organization.Models.Authentication.LoginPayload;
using SignUpPayload = Cuplan.Organization.Models.Authentication.SignUpPayload;

namespace Cuplan.Organization.Services;

public class Auth0Provider : IAuthProvider
{
    private const string ResourceOwnerGrant = "password";
    private const string RequiredScopes = "openid offline_access";
    private const string SignUpEndpoint = "dbconnections/signup";
    private const string LoginEndpoint = "oauth/token";
    private const string IdPrefix = "auth0|";

    private readonly HttpClient _client;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _database;
    private readonly string _identityProviderAudience;
    private readonly string _identityProviderUrl;

    public Auth0Provider(IConfiguration config, ISecretsManager secretsManager)
    {
        // Expects the identity provider url to finish with '/'.
        _identityProviderUrl = config["IdentityProvider:Authority"];
        _identityProviderAudience = config["IdentityProvider:Audience"];

        if (_identityProviderUrl is null) throw new NullReferenceException("'_identityProviderUrl' is null.");

        _clientId = secretsManager.Get(config["Auth0:ClientIdSecret"]);

        if (_clientId is null) throw new NullReferenceException("'_clientId' is null.");

        _clientSecret = secretsManager.Get(config["Auth0:ClientSecretSecret"]);

        if (_clientSecret is null) throw new NullReferenceException("'_clientSecret' is null.");

        _database = config["Auth0:Database"];

        if (_database is null) throw new NullReferenceException("'_database' is null.");

        _client = new HttpClient();
    }

    public async Task<Result<string, Error<ErrorKind>>> SignUp(SignUpPayload signUp)
    {
        ServiceModels.Auth0.SignUpPayload payload = new();
        payload.client_id = _clientId;
        payload.email = signUp.Email;
        payload.password = signUp.Password;
        payload.connection = _database;

        HttpResponseMessage response =
            await _client.PostAsync($"{_identityProviderUrl}{SignUpEndpoint}", JsonContent.Create(payload));
        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError,
                content));

        SignUpOkResponse okResponse = JsonConvert.DeserializeObject<SignUpOkResponse>(content);

        if (okResponse._id is null)
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError,
                "response contains no '_id'"));

        // the prefix is added so the ids represented within the tokens are equal to the ones used
        // within Cuplan.
        return Result<string, Error<ErrorKind>>.Ok($"{IdPrefix}{okResponse._id}");
    }

    public async Task<Result<LoginSuccessPayload, Error<ErrorKind>>> Login(LoginPayload login)
    {
        ServiceModels.Auth0.LoginPayload payload = new()
        {
            audience = _identityProviderAudience,
            client_id = _clientId,
            client_secret = _clientSecret,
            grant_type = ResourceOwnerGrant,
            username = login.Email,
            password = login.Password,
            scope = RequiredScopes
        };

        HttpResponseMessage response =
            await _client.PostAsync($"{_identityProviderUrl}{LoginEndpoint}", JsonContent.Create(payload));
        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.InvalidData,
                "invalid email or password"));

        if (response.StatusCode != HttpStatusCode.OK)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError,
                $"login result is not 'Ok': {content}"));

        LoginOkResponse? okResponse = JsonConvert.DeserializeObject<LoginOkResponse>(content);

        if (okResponse is null)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError,
                $"deserialized ok response is null: {content}"));

        LoginSuccessPayload successPayload = (LoginSuccessPayload)okResponse;

        return Result<LoginSuccessPayload, Error<ErrorKind>>.Ok(successPayload);
    }
}