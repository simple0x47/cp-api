using Core;
using Cuplan.Organization.Services;
using Organization;

namespace Cuplan.Organization.Models;

public class RoleManager
{
    private readonly IRoleRepository _repository;

    public RoleManager(IRoleRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<Result<Role, Error<ErrorKind>>> GetAdminRole()
    {
        return await _repository.GetAdminRole();
    }
}