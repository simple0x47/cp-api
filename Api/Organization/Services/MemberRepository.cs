using Core;
using Cuplan.Organization.Models;
using Cuplan.Organization.Transformers;
using MongoDB.Bson;
using MongoDB.Driver;
using Organization.Config;
using Member = Cuplan.Organization.ServiceModels.Member;

namespace Cuplan.Organization.Services;

public class MemberRepository : IMemberRepository
{
    private const double DefaultTimeoutAfterSeconds = 15;

    private readonly IMongoCollection<Member> _collection;

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
            .GetCollection<Member>(config.GetStringOrThrowException("MemberRepository:Collection"));
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

    public async Task<Result<Empty, Error<ErrorKind>>> SetPermissions(string memberId, IEnumerable<string> permissions)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            FilterDefinition<Member>? filter = Builders<Member>.Filter.Eq(m => m.Id, id);
            UpdateDefinition<Member>? update =
                Builders<Member>.Update.Set(m => m.Permissions, permissions);

            UpdateResult result = await _collection.UpdateOneAsync(filter, update)
                .WaitAsync(_setPermissionsTimeout);

            if (result.ModifiedCount != 1)
                return Result<Empty, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.NotFound,
                    $"member with id '{memberId}' not found"));

            return Result<Empty, Error<ErrorKind>>.Ok(new Empty());
        }
        catch (TimeoutException)
        {
            string message = "timed out setting permissions";
            _logger.LogInformation(message);
            return Result<Empty, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to set permissions: {e}";
            _logger.LogInformation(message);
            return Result<Empty, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<Empty, Error<ErrorKind>>> SetRoles(string memberId, IEnumerable<string> roles)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            FilterDefinition<Member>? filter = Builders<Member>.Filter.Eq(m => m.Id, id);
            UpdateDefinition<Member>? update =
                Builders<Member>.Update.Set(m => m.Roles, roles);

            UpdateResult result = await _collection.UpdateOneAsync(filter, update)
                .WaitAsync(_setRolesTimeout);

            return Result<Empty, Error<ErrorKind>>.Ok(new Empty());
        }
        catch (TimeoutException)
        {
            string message = "timed out setting roles";
            _logger.LogInformation(message);
            return Result<Empty, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to set roles: {e}";
            _logger.LogInformation(message);
            return Result<Empty, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<string, Error<ErrorKind>>> Create(PartialMember partialMember)
    {
        try
        {
            Member member = new(partialMember);

            await _collection.InsertOneAsync(member).WaitAsync(_createTimeout);
            return Result<string, Error<ErrorKind>>.Ok(member.Id.ToString());
        }
        catch (TimeoutException)
        {
            string message = "timed out creating member";
            _logger.LogInformation(message);
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut, message));
        }
        catch (Exception e)
        {
            string message = $"failed to create member: {e}";
            _logger.LogInformation(message);
            return Result<string, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError, message));
        }
    }

    public async Task<Result<Models.Member, Error<ErrorKind>>> FindById(string memberId)
    {
        try
        {
            ObjectId id = ObjectId.Parse(memberId);
            IAsyncCursor<Member>? cursor =
                await _collection.FindAsync(p => p.Id.Equals(id))
                    .WaitAsync(_findByIdTimeout);

            if (cursor is null)
                return Result<Models.Member, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                    "find async cursor is null"));

            bool hasNext = await cursor.MoveNextAsync().WaitAsync(_findByIdTimeout);

            if (!hasNext)
                return Result<Models.Member, Error<ErrorKind>>.Err(
                    new Error<ErrorKind>(ErrorKind.NotFound, $"could not find member by id '{memberId}'"));

            Member? idMember = cursor.Current.FirstOrDefault();

            if (idMember is null)
                return Result<Models.Member, Error<ErrorKind>>.Err(
                    new Error<ErrorKind>(ErrorKind.NotFound, $"could not find member by id '{memberId}'"));

            Result<Models.Member, Error<ErrorKind>> modelsMemberResult = await idMember.Transform(_roleRepository);

            if (!modelsMemberResult.IsOk)
                return Result<Models.Member, Error<ErrorKind>>.Err(modelsMemberResult.UnwrapErr());

            return Result<Models.Member, Error<ErrorKind>>.Ok(modelsMemberResult.Unwrap());
        }
        catch (TimeoutException)
        {
            string message = "timed out finding member by id";
            _logger.LogInformation(message);
            return Result<Models.Member, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find member by id: {e}";
            _logger.LogInformation(message);
            return Result<Models.Member, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                message));
        }
    }

    public async Task<Result<Models.Member[], Error<ErrorKind>>> FindByUserId(string userId)
    {
        try
        {
            IList<Member> members =
                await _collection.Find(p => p.UserId.Equals(userId)).ToListAsync()
                    .WaitAsync(_findByUserIdTimeout);

            Models.Member[] modelsMembers = new Models.Member[members.Count];
            int i = 0;

            foreach (Member member in members)
            {
                Result<Models.Member, Error<ErrorKind>> modelsMemberResult = await member.Transform(_roleRepository);

                if (!modelsMemberResult.IsOk)
                    return Result<Models.Member[], Error<ErrorKind>>.Err(modelsMemberResult.UnwrapErr());

                modelsMembers[i] = modelsMemberResult.Unwrap();
                i++;
            }

            return Result<Models.Member[], Error<ErrorKind>>.Ok(modelsMembers);
        }
        catch (TimeoutException)
        {
            string message = "timed out finding members by user id";
            _logger.LogInformation(message);
            return Result<Models.Member[], Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.TimedOut,
                message));
        }
        catch (Exception e)
        {
            string message = $"failed to find members by user id: {e}";
            _logger.LogInformation(message);
            return Result<Models.Member[], Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.StorageError,
                message));
        }
    }
}