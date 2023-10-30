using Core;
using Cuplan.Organization.Models;

namespace Cuplan.Organization.Services;

public interface IAuthProvider
{
    /// <summary>
    ///     Signs up an user for the specified payload.
    /// </summary>
    /// <param name="signUp"></param>
    /// <returns>User's id or an error.</returns>
    public Task<Result<string, Error<ErrorKind>>> SignUp(SignUpPayload signUp);
}