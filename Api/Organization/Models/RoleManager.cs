using Core;
using Cuplan.Organization.Services;

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
    public async Task<Result<Role, Error<string>>> GetAdminRole()
    {
        return await _repository.GetAdminRole();
    }
}