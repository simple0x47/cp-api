using Core;
using Cuplan.Organization.ControllerModels;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Cuplan.Organization.Controllers;

[ApiController]
public class MemberController : ControllerBase
{
    private readonly MemberManager _memberManager;

    public MemberController(MemberManager memberManager)
    {
        _memberManager = memberManager;
    }

    [Route("api/[controller]")]
    [HttpPost]
    [Authorize]
    [DevOnly]
    public async Task<IActionResult> Create([FromBody] PartialMember partialMember)
    {
        Result<string, Error<string>> createMemberResult = await _memberManager.Create(partialMember);

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
        Result<Member, Error<string>> readResult = await _memberManager.Read(id);

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
    public async Task<IActionResult> Update([FromBody] Member idMember)
    {
        Result<Empty, Error<string>> updateResult = await _memberManager.Update(idMember);

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
    [Authorize]
    public async Task<IActionResult> ReadMembersByUserId([FromRoute] string userId)
    {
        Result<Member[], Error<string>> result = await _memberManager.ReadMembersByUserId(userId);

        if (!result.IsOk)
        {
            Error<string> error = result.UnwrapErr();

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        // WrappedResult required because Javascript JSON cannot handle deserializing directly empty arrays if they
        // are not contained within an object.
        return Ok(JsonConvert.SerializeObject(new WrappedResult<Member[]>(result.Unwrap())));
    }
}