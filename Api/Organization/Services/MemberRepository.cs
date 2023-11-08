using Core;
using Cuplan.Organization.Models;
using Cuplan.Organization.Transformers;
using MongoDB.Bson;
using MongoDB.Driver;
using Organization.Config;
using Membership = Cuplan.Organization.ServiceModels.Membership;

namespace Cuplan.Organization.Services;

public class MemberRepository : IMemberRepository
{
    private const double DefaultTimeoutAfterSeconds = 15;

    private readonly IMongoCollection<Membership> _collection;

    private readonly TimeSpan _createTimeout;
    private readonly TimeSpan _findByIdTimeout;
    private readonly TimeSpan _findByUserIdTimeout;
    private readonly ILogger<MemberRepository> _logger;
    private readonly IRoleRepository _roleRepository;
    private readonly TimeSpan _setPermissionsTimeout;
    private readonly TimeSpan _setRolesTimeout;

    public MemberRepository(ILogger<MemberRepository> logger, ConfigurationReader config, MongoClient client,
        IRoleRepository roleRepository)
    {
        _logger = logger;
        _collection = client.GetDatabase(config.GetStringOrThrowException(ConfigurationReader.DatabaseKey))
            .GetCollection<Membership>(config.GetStringOrThrowException("MemberRepository:Collection"));
        _roleRepository = roleRepository;

        _createTimeout =
            TimeSpan.FromSeconds(
                config.GetDoubleOrDefault("MemberRepository:CreateTimeout", DefaultTimeoutAfterSeconds));
        _findByIdTimeout =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("MemberRepository:FindByIdTimeout",
                DefaultTimeoutAfterSeconds));
        _findByUserIdTimeout =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("MemberRepository:FindByUserIdTimeout",
                DefaultTimeoutAfterSeconds));
        _setPermissionsTimeout =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("MemberRepository:SetPermissionsTimeout",
                DefaultTimeoutAfterSeconds));
        _setRolesTimeout =
            TimeSpan.FromSeconds(config.GetDoubleOrDefault("MemberRepository:SetRolesTimeout",
                DefaultTimeoutAfterSeconds));
    }

    public async Task<Result<Empty, Error<string>>> SetPermissions(string memberId, IEnumerable<string> permissions)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            FilterDefinition<Membership>? filter = Builders<Membership>.Filter.Eq(m => m.Id, id);
            UpdateDefinition<Membership>? update =
                Builders<Membership>.Update.Set(m => m.Permissions, permissions);

            UpdateResult result = await _collection.UpdateOneAsync(filter, update)
                .WaitAsync(_setPermissionsTimeout);

            if (result.ModifiedCount != 1)
                return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.NotFound,
                    $"member with id '{memberId}' not found"));

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (TimeoutException)
        {
            string message = "timed out setting permissions";
            _logger.LogInformation(message);
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to set permissions: {e}";
            _logger.LogInformation(message);
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<Empty, Error<string>>> SetRoles(string memberId, IEnumerable<string> roles)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            FilterDefinition<Membership>? filter = Builders<Membership>.Filter.Eq(m => m.Id, id);
            UpdateDefinition<Membership>? update =
                Builders<Membership>.Update.Set(m => m.Roles, roles);

            UpdateResult result = await _collection.UpdateOneAsync(filter, update)
                .WaitAsync(_setRolesTimeout);

            return Result<Empty, Error<string>>.Ok(new Empty());
        }
        catch (TimeoutException)
        {
            string message = "timed out setting roles";
            _logger.LogInformation(message);
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to set roles: {e}";
            _logger.LogInformation(message);
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<string, Error<string>>> Create(PartialMembership partialMembership)
    {
        try
        {
            Membership membership = new(partialMembership);

            await _collection.InsertOneAsync(membership).WaitAsync(_createTimeout);
            return Result<string, Error<string>>.Ok(membership.Id.ToString());
        }
        catch (TimeoutException)
        {
            string message = "timed out creating member";
            _logger.LogInformation(message);
            return Result<string, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to create member: {e}";
            _logger.LogInformation(message);
            return Result<string, Error<string>>.Err(new Error<string>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<Models.Membership, Error<string>>> FindById(string memberId)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            IAsyncCursor<Membership>? cursor =
                await _collection.FindAsync(p => p.Id.Equals(id))
                    .WaitAsync(_findByIdTimeout);

            if (cursor is null)
                return Result<Models.Membership, Error<string>>.Err(new Error<string>(ErrorKind.StorageError,
                    "find async cursor is null"));

            bool hasNext = await cursor.MoveNextAsync().WaitAsync(_findByIdTimeout);

            if (!hasNext)
                return Result<Models.Membership, Error<string>>.Err(
                    new Error<string>(ErrorKind.NotFound, $"could not find member by id '{memberId}'"));

            Membership? idMember = cursor.Current.FirstOrDefault();

            if (idMember is null)
                return Result<Models.Membership, Error<string>>.Err(
                    new Error<string>(ErrorKind.NotFound, $"could not find member by id '{memberId}'"));

            Result<Models.Membership, Error<string>> modelsMemberResult = await idMember.Transform(_roleRepository);

            if (!modelsMemberResult.IsOk)
                return Result<Models.Membership, Error<string>>.Err(modelsMemberResult.UnwrapErr());

            return Result<Models.Membership, Error<string>>.Ok(modelsMemberResult.Unwrap());
        }
        catch (TimeoutException)
        {
            string message = "timed out finding member by id";
            _logger.LogInformation(message);
            return Result<Models.Membership, Error<string>>.Err(new Error<string>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find member by id: {e}";
            _logger.LogInformation(message);
            return Result<Models.Membership, Error<string>>.Err(new Error<string>(ErrorKind.StorageError,
                message));
        }
    }

    public async Task<Result<Models.Membership[], Error<string>>> FindByUserId(string userId)
    {
        try
        {
            IList<Membership> members =
                await _collection.Find(p => p.UserId.Equals(userId)).ToListAsync()
                    .WaitAsync(_findByUserIdTimeout);

            Models.Membership[] modelsMembers = new Models.Membership[members.Count];
            int i = 0;

            foreach (Membership member in members)
            {
                Result<Models.Membership, Error<string>> modelsMemberResult = await member.Transform(_roleRepository);

                if (!modelsMemberResult.IsOk)
                    return Result<Models.Membership[], Error<string>>.Err(modelsMemberResult.UnwrapErr());

                modelsMembers[i] = modelsMemberResult.Unwrap();
                i++;
            }

            return Result<Models.Membership[], Error<string>>.Ok(modelsMembers);
        }
        catch (TimeoutException)
        {
            string message = "timed out finding members by user id";
            _logger.LogInformation(message);
            return Result<Models.Membership[], Error<string>>.Err(new Error<string>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find members by user id: {e}";
            _logger.LogInformation(message);
            return Result<Models.Membership[], Error<string>>.Err(new Error<string>(ErrorKind.StorageError,
                message));
        }
    }
}