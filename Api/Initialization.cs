using Core.Secrets;

namespace Organization;

public class Initialization
{
    private readonly WebApplicationBuilder _builder;
    private readonly ISecretsManager _secretsManager;

    public Initialization(WebApplicationBuilder builder)
    {
        _builder = builder;
        _secretsManager = new BitwardenSecretsManager(null);
    }

    public string GetMongoDbUri()
    {
        string? secretId = _builder.Configuration.GetValue<string>("MongoDBConnectionUriSecret");

        if (secretId is null)
            throw new InvalidOperationException("Missing 'MongoDBConnectionUriSecret' from configuration.");

        string? mongoDbUri = _secretsManager.Get(secretId);

        if (mongoDbUri is null)
            throw new InvalidOperationException(
                $"Failed to retrieve secret containing the 'MongoDBUri' for secret id '{secretId}'");

        return mongoDbUri;
    }
}