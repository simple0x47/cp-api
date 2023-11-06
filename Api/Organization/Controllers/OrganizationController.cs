using Core;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuplan.Organization.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrganizationController
    : ControllerBase
{
    private readonly ILogger<OrganizationController> _logger;
    private readonly OrganizationManager _orgManager;

    public OrganizationController(ILogger<OrganizationController> logger, OrganizationManager orgManager)
    {
        _logger = logger;
        _orgManager = orgManager;
    }

    [HttpPost]
    [Authorize]
    [DevOnly]
    public async Task<IActionResult> Post([FromBody] PartialOrganization org)
    {
        Result<string, Error<string>> result = await _orgManager.Create(org);

        return result.Match<IActionResult>(
            orgId => { return Ok(orgId); },
            error =>
            {
                _logger.LogWarning($"failed to create organization: {error.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        );
    }
}