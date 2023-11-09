using Core;
using Cuplan.Organization.Models.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Cuplan.Organization.Controllers;

[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly Authenticator _authenticator;

    public AuthenticationController(Authenticator authenticator)
    {
        _authenticator = authenticator;
    }

    [Route("api/[controller]/register-creating-org")]
    [EnableCors("Frontend")]
    [HttpPost]
    public async Task<IActionResult> RegisterCreatingOrg([FromBody] RegisterCreatingOrgPayload payload)
    {
        Result<LoginSuccessPayload, Error<string>> result = await _authenticator.RegisterCreatingOrg(payload);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
        }

        return Ok(JsonConvert.SerializeObject(result.Unwrap()));
    }

    [Route("api/[controller]/login")]
    [EnableCors("Frontend")]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginPayload payload)
    {
        Result<LoginSuccessPayload, Error<string>> result = await _authenticator.Login(payload);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
        }

        return Ok(JsonConvert.SerializeObject(result.Unwrap()));
    }

    [Route("api/[controller]/forgot-password")]
    [EnableCors("Frontend")]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordPayload payload)
    {
        Result<Empty, Error<string>> result = await _authenticator.ForgotPassword(payload);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.ErrorKind);
        }

        return NoContent();
    }

    [Route("api/[controller]/refresh-token")]
    [EnableCors("Frontend")]
    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        Result<LoginSuccessPayload, Error<string>> result = await _authenticator.RefreshToken(refreshToken);

        if (!result.IsOk) return StatusCode(StatusCodes.Status500InternalServerError, result.UnwrapErr().ErrorKind);

        return Ok(result.Unwrap());
    }
}