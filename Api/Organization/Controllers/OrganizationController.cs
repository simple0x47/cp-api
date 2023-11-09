using Core;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuplan.Organization.Controllers;

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

    [Route("api/[controller]")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] PartialOrganization org)
    {
        Result<string, Error<string>> result = await _orgManager.Create(org);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            _logger.LogWarning($"failed to create organization: {error.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(result.Unwrap());
    }
}