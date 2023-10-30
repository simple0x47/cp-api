using Core;
using Cuplan.Organization.ServiceModels;
using MongoDB.Driver;
using Organization;
using Organization.Config;

namespace Cuplan.Organization.Services;

public class RoleRepository : IRoleRepository
{
    private const double DefaultTimeoutAfterSeconds = 15;

    private readonly IMongoCollection<Role> _collection;

    private readonly TimeSpan _getAdminRoleIdTimeout;

    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(ILogger<RoleRepository> logger, ConfigurationReader config, MongoClient client)
    {
        _logger = logger;
        _collection = client.GetDatabase(config.GetStringOrThrowException(ConfigurationReader.DatabaseKey))
            .GetCollection<Role>(config.GetStringOrThrowException("RoleRepository:Collection"));

        _getAdminRoleIdTimeout =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("GetAdminRoleIdTimeout", DefaultTimeoutAfterSeconds));
    }

    public async Task<Result<Models.Role, Error<ErrorKind>>> GetAdminRole()
    {
        try
        {
            IAsyncCursor<Role>? cursor = await _collection.FindAsync(r => r.DefaultAdmin == true)
                .WaitAsync(_getAdminRoleIdTimeout);

            if (cursor is null)
                return Result<Models.Role, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                    "find async cursor is null"));

            bool hasNext = await cursor.MoveNextAsync().WaitAsync(_getAdminRoleIdTimeout);

            if (!hasNext)
                return Result<Models.Role, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.NotFound,
                    "could not find the admin role"));

            Role? role = cursor.Current.FirstOrDefault();

            if (role is null)
                return Result<Models.Role, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.NotFound,
                    "could not find the admin role"));

            return Result<Models.Role, Error<ErrorKind>>.Ok(role);
        }
        catch (TimeoutException)
        {
            string message = "timed out getting the admin role";
            _logger.LogInformation(message);
            return Result<Models.Role, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to get the admin role: {e}";
            _logger.LogInformation(message);
            return Result<Models.Role, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                message));
        }
    }
}