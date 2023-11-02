using Core;
using Cuplan.Organization.Models;

namespace Cuplan.Organization.Services;

public interface IRoleRepository
{
    /// <summary>
    ///     Gets the admin role.
    /// </summary>
    /// <returns>The admin <see cref="Role" /> or an error.</returns>
    public Task<Result<Role, Error<ErrorKind>>> GetAdminRole();

    /// <summary>
    ///     Gets roles by their ids.
    /// </summary>
    /// <param name="id">Ids of the roles to be retrieved.</param>
    /// <returns>An enumerable of <see cref="Role" /> or an error.</returns>
    public Task<Result<IEnumerable<Role>, Error<ErrorKind>>> FindByIds(IEnumerable<string> id);
}