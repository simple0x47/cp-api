using System.Net;
using Core;
using Core.Secrets;
using Cuplan.Organization.Models;
using Cuplan.Organization.ServiceModels;
using Newtonsoft.Json;

namespace Cuplan.Organization.Services;

public class Auth0Provider : IAuthProvider
{
    private const string SignUpEndpoint = "dbconnections/signup";

    private readonly HttpClient _client;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _database;
    private readonly string _identityProviderUrl;

    public Auth0Provider(IConfiguration config, ISecretsManager secretsManager)
    {
        // Expects the identity provider url to finish with '/'.
        _identityProviderUrl = config["IdentityProvider:Authority"];

        if (_identityProviderUrl is null) throw new NullReferenceException("'_identityProviderUrl' is null.");

        _clientId = secretsManager.get(config["Auth0:ClientIdSecret"]);

        if (_clientId is null) throw new NullReferenceException("'_clientId' is null.");

        _clientSecret = secretsManager.get(config["Auth0:ClientSecretSecret"]);

        if (_clientSecret is null) throw new NullReferenceException("'_clientSecret' is null.");

        _database = config["Auth0:Database"];

        if (_database is null) throw new NullReferenceException("'_database' is null.");

        _client = new HttpClient();
    }

    public async Task<Result<string, Error<ErrorKind>>> SignUp(SignUpPayload signUp)
    {
        Auth0SignUpPayload payload = new();
        payload.client_id = _clientId;
        payload.email = signUp.Email;
        payload.password = signUp.Password;
        payload.connection = _database;

        HttpResponseMessage response =
            await _client.PostAsync($"{_identityProviderUrl}{SignUpEndpoint}", JsonContent.Create(payload));
        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode != HttpStatusCode.OK)
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError, content));

        Auth0SignUpOkResponse okResponse = JsonConvert.DeserializeObject<Auth0SignUpOkResponse>(content);

        if (okResponse._id is null)
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.ServiceError,
                "response contains no '_id'"));

        return Result<string, Error<ErrorKind>>.Ok(okResponse._id);
    }
}