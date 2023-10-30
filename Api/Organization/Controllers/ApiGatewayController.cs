using Core;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Mvc;
using Organization;

namespace Cuplan.Organization.Controllers;

[ApiController]
public class ApiGatewayController : ControllerBase
{
    private readonly ApiGatewayLogic _apiGatewayLogic;

    public ApiGatewayController(ApiGatewayLogic apiGatewayLogic)
    {
        _apiGatewayLogic = apiGatewayLogic;
    }

    [Route("api/[controller]/register-creating-org")]
    [HttpPost]
    public async Task<IActionResult> RegisterCreatingOrg([FromBody] RegisterCreatingOrgPayload payload)
    {
        Result<string, Error<ErrorKind>> result = await _apiGatewayLogic.RegisterCreatingOrg(payload);

        if (!result.IsOk)
        {
            Error<ErrorKind> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(result.Unwrap());
    }
}