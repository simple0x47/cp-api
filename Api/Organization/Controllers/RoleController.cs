using Core;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Organization;

namespace Cuplan.Organization.Controllers;

[ApiController]
public class RoleController : ControllerBase
{
    private readonly RoleManager _roleManager;

    public RoleController(RoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    [Route("api/[controller]/AdminRole")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAdminRole()
    {
        Result<Role, Error<ErrorKind>> getAdminRoleResult = await _roleManager.GetAdminRole();

        if (!getAdminRoleResult.IsOk)
        {
            Error<ErrorKind> error = getAdminRoleResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return NotFound(error.Message);

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(JsonConvert.SerializeObject(getAdminRoleResult.Unwrap()));
    }
}