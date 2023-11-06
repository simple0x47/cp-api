using System.Net;
using Core;
using Core.Secrets;
using Cuplan.Organization.Models.Authentication;
using Cuplan.Organization.ServiceModels.Auth0;
using LoginPayload = Cuplan.Organization.Models.Authentication.LoginPayload;
using SignUpPayload = Cuplan.Organization.Models.Authentication.SignUpPayload;

namespace Cuplan.Organization.Services;

public class Auth0Provider : IAuthProvider
{
    private const string ResourceOwnerGrant = "password";
    private const string RequiredScopes = "openid offline_access";
    private const string SignUpEndpoint = "dbconnections/signup";
    private const string LoginEndpoint = "oauth/token";
    private const string ChangePasswordEndpoint = "dbconnections/change_password";
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

    public async Task<Result<string, Error<string>>> SignUp(SignUpPayload signUp)
    {
        ServiceModels.Auth0.SignUpPayload payload = new();
        payload.client_id = _clientId;
        payload.email = signUp.Email;
        payload.password = signUp.Password;
        payload.connection = _database;

        HttpResponseMessage response =
            await _client.PostAsync($"{_identityProviderUrl}{SignUpEndpoint}", JsonContent.Create(payload));

        if (response.StatusCode != HttpStatusCode.OK)
            return await HandleRegisterAuth0ErrorResponse(response);

        SignUpOkResponse? okResponse = await response.Content.ReadFromJsonAsync<SignUpOkResponse>();

        if (okResponse is null)
            return Result<string, Error<string>>.Err(new Error<string>(ErrorKind.OkResponseNull,
                "deserialized ok response is null"));

        // the prefix is added so the ids represented within the tokens are equal to the ones used
        // within Cuplan.
        return Result<string, Error<string>>.Ok($"{IdPrefix}{okResponse.Value._id}");
    }

    public async Task<Result<LoginSuccessPayload, Error<string>>> Login(LoginPayload login)
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

        if (response.StatusCode != HttpStatusCode.OK)
            return await HandleLoginAuth0ErrorResponse(response);

        LoginOkResponse? okResponse = await response.Content.ReadFromJsonAsync<LoginOkResponse>();

        if (okResponse is null)
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.OkResponseNull,
                "deserialized ok response is null"));

        LoginSuccessPayload successPayload = (LoginSuccessPayload)okResponse;

        return Result<LoginSuccessPayload, Error<string>>.Ok(successPayload);
    }

    public async Task<Result<Empty, Error<string>>> ForgotPassword(ForgotPasswordPayload forgotPassword)
    {
        ChangePasswordPayload payload = new()
        {
            client_id = _clientId,
            connection = _database,
            email = forgotPassword.Email
        };

        HttpResponseMessage response = await _client.PostAsync($"{_identityProviderUrl}{ChangePasswordEndpoint}",
            JsonContent.Create(payload));

        if (response.StatusCode != HttpStatusCode.OK)
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.UnknownError,
                "Auth0 replied with a non-ok response"));

        return Result<Empty, Error<string>>.Ok(new Empty());
    }

    private async Task<Result<string, Error<string>>> HandleRegisterAuth0ErrorResponse(HttpResponseMessage response)
    {
        RegisterAuth0ErrorResponse errorResponse =
            await response.Content.ReadFromJsonAsync<RegisterAuth0ErrorResponse>();

        if (errorResponse.code is null)
            return Result<string, Error<string>>.Err(new Error<string>(ErrorKind.ErrorResponseNull,
                "deserialized 'code' is null"));

        return Result<string, Error<string>>.Err(new Error<string>(errorResponse.code, errorResponse.description));
    }

    private async Task<Result<LoginSuccessPayload, Error<string>>> HandleLoginAuth0ErrorResponse(
        HttpResponseMessage response)
    {
        LoginAuth0ErrorResponse errorResponse = await response.Content.ReadFromJsonAsync<LoginAuth0ErrorResponse>();

        if (errorResponse.error is null)
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.ErrorResponseNull,
                "deserialized 'error' is null"));

        return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(errorResponse.error,
            errorResponse.error_description));
    }
}