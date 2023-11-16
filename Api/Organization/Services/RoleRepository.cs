using Core;
using Cuplan.Organization.ServiceModels;
using MongoDB.Driver;
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

    public async Task<Result<Models.Role, Error<string>>> GetAdminRole()
    {
        try
        {
            IAsyncCursor<Role>? cursor = await _collection.FindAsync(r => r.DefaultAdmin == true)
                .WaitAsync(_getAdminRoleIdTimeout);

            if (cursor is null)
                return Result<Models.Role, Error<string>>.Err(new Error<string>(ErrorKind.ServiceError,
                    "find async cursor is null"));

            bool hasNext = await cursor.MoveNextAsync().WaitAsync(_getAdminRoleIdTimeout);

            if (!hasNext)
                return Result<Models.Role, Error<string>>.Err(new Error<string>(ErrorKind.NotFound,
                    "could not find the admin role"));

            Role? role = cursor.Current.FirstOrDefault();

            if (role is null)
                return Result<Models.Role, Error<string>>.Err(new Error<string>(ErrorKind.NotFound,
                    "could not find the admin role"));

            return Result<Models.Role, Error<string>>.Ok(role);
        }
        catch (TimeoutException)
        {
            string message = "timed out getting the admin role";
            _logger.LogInformation(message);
            return Result<Models.Role, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to get the admin role: {e}";
            _logger.LogInformation(message);
            return Result<Models.Role, Error<string>>.Err(new Error<string>(ErrorKind.ServiceError,
                message));
        }
    }

    public async Task<Result<IEnumerable<Models.Role>, Error<string>>> FindByIds(IEnumerable<string> ids)
    {
        try
        {
            IList<Role>? roles = await _collection.Find(r => ids.Contains(r.Id.ToString())).ToListAsync()
                .WaitAsync(_getAdminRoleIdTimeout);

            if (roles is null)
                return Result<IEnumerable<Models.Role>, Error<string>>.Err(new Error<string>(
                    ErrorKind.ServiceError,
                    "'roles' is null"));

            IList<Models.Role> modelsRole = new List<Models.Role>();

            foreach (Role role in roles) modelsRole.Add(role);

            return Result<IEnumerable<Models.Role>, Error<string>>.Ok(modelsRole);
        }
        catch (TimeoutException)
        {
            string message = "timed out finding roles by ids";
            _logger.LogInformation(message);
            return Result<IEnumerable<Models.Role>, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find roles by ids: {e}";
            _logger.LogInformation(message);
            return Result<IEnumerable<Models.Role>, Error<string>>.Err(new Error<string>(ErrorKind.ServiceError,
                message));
        }
    }
}