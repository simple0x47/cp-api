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
    public async Task<Result<string, Error<string>>> Create(PartialOrganization org)
    {
        // Avoid permission injection at creation.
        org.Permissions = Array.Empty<string>();

        Result<string, Error<string>> result = await _repository.Create(org);

        return result;
    }
}