using Core;
using Cuplan.Organization.Models;
using Organization;

namespace Cuplan.Organization.Services;

public interface IOrganizationRepository
{
    /// <summary>
    ///     Create an organization, returning its id if the operation has been completed successfully.
    /// </summary>
    /// <param name="partialOrg">The organization to be created.</param>
    /// <returns>Id of the created organization or an error.</returns>
    public Task<Result<string, Error<ErrorKind>>> Create(PartialOrganization partialOrg);

    /// <summary>
    ///     Finds an organization by its id, returning the organization if it is found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The organization whose id has been specified or an error.</returns>
    public Task<Result<Models.Organization, Error<ErrorKind>>> FindById(string id);
}