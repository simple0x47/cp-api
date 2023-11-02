using Core;
using Cuplan.Organization.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Organization.Config;

namespace Cuplan.Organization.Services;

public class OrganizationRepository : IOrganizationRepository
{
    private const double DefaultTimeoutAfterSeconds = 15;
    private readonly IMongoCollection<ServiceModels.Organization> _collection;

    private readonly TimeSpan _createTimeoutAfterSeconds;
    private readonly TimeSpan _findByIdTimeoutAfterSeconds;

    private readonly ILogger<OrganizationRepository> _logger;

    public OrganizationRepository(ILogger<OrganizationRepository> logger, ConfigurationReader config,
        MongoClient client)
    {
        _logger = logger;
        _collection = client.GetDatabase(config.GetStringOrThrowException(ConfigurationReader.DatabaseKey))
            .GetCollection<ServiceModels.Organization>(
                config.GetStringOrThrowException("OrganizationRepository:Collection"));

        _createTimeoutAfterSeconds =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("OrganizationRepository:CreateTimeout",
                DefaultTimeoutAfterSeconds));
        _findByIdTimeoutAfterSeconds =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("OrganizationRepository:FindByIdTimeout",
                DefaultTimeoutAfterSeconds));
    }

    public async Task<Result<string, Error<ErrorKind>>> Create(PartialOrganization partialOrg)
    {
        try
        {
            ServiceModels.Organization org = new(partialOrg);
            await _collection.InsertOneAsync(org).WaitAsync(_createTimeoutAfterSeconds);

            return Result<string, Error<ErrorKind>>.Ok(org.Id.ToString());
        }
        catch (Exception e)
        {
            string message = $"failed to insert organization: {e}";
            _logger.LogInformation(message);
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                message));
        }
    }

    public async Task<Result<Models.Organization, Error<ErrorKind>>> FindById(string orgId)
    {
        try
        {
            ObjectId id = ObjectId.Parse(orgId);
            IAsyncCursor<ServiceModels.Organization>? cursor =
                await _collection.FindAsync(p => p.Id.Equals(id))
                    .WaitAsync(_findByIdTimeoutAfterSeconds);

            if (cursor is null)
                return Result<Models.Organization, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                    "find async cursor is null"));

            bool hasNext = await cursor.MoveNextAsync().WaitAsync(_findByIdTimeoutAfterSeconds);

            if (!hasNext)
                return Result<Models.Organization, Error<ErrorKind>>.Err(
                    new Error<ErrorKind>(ErrorKind.NotFound, $"could not find org by id '{id}'"));

            ServiceModels.Organization? organization = cursor.Current.FirstOrDefault();

            if (organization is null)
                return Result<Models.Organization, Error<ErrorKind>>.Err(
                    new Error<ErrorKind>(ErrorKind.NotFound, $"could not find org by id '{id}'"));

            return Result<Models.Organization, Error<ErrorKind>>.Ok(organization);
        }
        catch (TimeoutException)
        {
            string message = "timed out finding organization by id";
            _logger.LogInformation(message);
            return Result<Models.Organization, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find organization by id: {e}";
            _logger.LogInformation(message);
            return Result<Models.Organization, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                message));
        }
    }
}