using Core;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Models;

public class OrganizationManager
{
    private readonly IOrganizationRepository _repository;

    public OrganizationManager(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    ///     Creates the organization.
    /// </summary>
    /// <param name="org"></param>
    /// <returns>Organization's id or an error.</returns>
    public async Task<Result<string, Error<ErrorKind>>> Create(PartialOrganization org)
    {
        Result<string, Error<ErrorKind>> result = await _repository.Create(org);

        return result;
    }
}