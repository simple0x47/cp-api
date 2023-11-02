using Core;
using Cuplan.Organization.Models;

namespace Cuplan.Organization.Services;

public interface IRoleRepository
{
    /// <summary>
    ///     Gets the admin role.
    /// </summary>
    /// <returns>An <see cref="Role" /> or an error.</returns>
    public Task<Result<Role, Error<ErrorKind>>> GetAdminRole();
}