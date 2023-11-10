using Core;
using Cuplan.Organization.ControllerModels;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Cuplan.Organization.Controllers;

[ApiController]
public class MembershipController : ControllerBase
{
    private readonly MembershipManager _membershipManager;

    public MembershipController(MembershipManager membershipManager)
    {
        _membershipManager = membershipManager;
    }

    [Route("api/[controller]")]
    [HttpPost]
    [Authorize]
    [DevOnly]
    public async Task<IActionResult> Create([FromBody] PartialMembership partialMembership)
    {
        Result<string, Error<string>> createMemberResult = await _membershipManager.Create(partialMembership);

        if (!createMemberResult.IsOk)
        {
            Error<string> error = createMemberResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("org id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(createMemberResult.Unwrap());
    }

    [Route("api/[controller]/{id}")]
    [HttpGet]
    [Authorize]
    [DevOnly]
    public async Task<IActionResult> Read([FromRoute] string id)
    {
        Result<Membership, Error<string>> readResult = await _membershipManager.Read(id);

        if (!readResult.IsOk)
        {
            Error<string> error = readResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("member id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(JsonConvert.SerializeObject(readResult.Unwrap()));
    }

    [Route("api/[controller]")]
    [HttpPatch]
    [Authorize]
    [DevOnly]
    public async Task<IActionResult> Update([FromBody] Membership idMembership)
    {
        Result<Empty, Error<string>> updateResult = await _membershipManager.Update(idMembership);

        if (!updateResult.IsOk)
        {
            Error<string> error = updateResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("member id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return NoContent();
    }

    [Route("api/[controller]/user/{userId}")]
    [HttpGet]
    [EnableCors("Frontend")]
    [Authorize]
    public async Task<IActionResult> ReadMembersByUserId([FromRoute] string userId)
    {
        Result<Membership[], Error<string>> result = await _membershipManager.ReadMembersByUserId(userId);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        // WrappedResult required because Javascript JSON cannot handle deserializing directly empty arrays if they
        // are not contained within an object.
        return Ok(JsonConvert.SerializeObject(new WrappedResult<Membership[]>(result.Unwrap())));
    }
}