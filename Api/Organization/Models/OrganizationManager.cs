using Core;
using Cuplan.Organization.Services;
using Organization;

namespace Cuplan.Organization.Models;

public class OrganizationManager(IOrganizationRepository repository)
{
    /// <summary>
    ///     Creates the organization.
    /// </summary>
    /// <param name="org"></param>
    /// <returns>Organization's id or an error.</returns>
    public async Task<Result<string, Error<ErrorKind>>> Create(PartialOrganization org)
    {
        Result<string, Error<ErrorKind>> result = await repository.Create(org);

        return result;
    }
}