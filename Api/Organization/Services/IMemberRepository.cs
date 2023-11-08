using Core;
using Cuplan.Organization.Models;

namespace Cuplan.Organization.Services;

public interface IMemberRepository
{
    /// <summary>
    ///     Creates a membership between an user id and an organization id.
    /// </summary>
    /// <param name="partialMembership">The membership to be created.</param>
    /// <returns>Id of the created member, null if the creation failed.</returns>
    public Task<Result<string, Error<string>>> Create(PartialMembership partialMembership);

    /// <summary>
    ///     Sets the permissions to a membership.
    /// </summary>
    /// <param name="memberId">Id of the membership.</param>
    /// <param name="permissions">Permissions to be set to the membership.</param>
    /// <returns>An <see cref="Empty" /> result indicating that the operation was successful, or an error.</returns>
    public Task<Result<Empty, Error<string>>> SetPermissions(string memberId, IEnumerable<string> permissions);

    /// <summary>
    ///     Sets the roles to a membership.
    /// </summary>
    /// <param name="memberId">Id of the membership.</param>
    /// <param name="roles">Roles to be set to the membership.</param>
    /// <returns>An <see cref="Empty" /> result indicating that the operation was successful, or an error.</returns>
    public Task<Result<Empty, Error<string>>> SetRoles(string memberId, IEnumerable<string> roles);

    /// <summary>
    ///     Finds a membership by its id.
    /// </summary>
    /// <param name="memberId">Id of the membership to be retrieved.</param>
    /// <returns>A <see cref="Membership" /> indicating that the operation was successful, or an error.</returns>
    public Task<Result<Membership, Error<string>>> FindById(string memberId);

    /// <summary>
    ///     Finds the memberships of an user by its id.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>An array of <see cref="Membership" /> or an error.</returns>
    public Task<Result<Membership[], Error<string>>> FindByUserId(string userId);
}