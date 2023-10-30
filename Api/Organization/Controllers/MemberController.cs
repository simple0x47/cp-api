using Core;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Organization;

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
    public async Task<IActionResult> Create([FromBody] PartialMember partialMember)
    {
        Result<string, Error<ErrorKind>> createMemberResult = await _memberManager.Create(partialMember);

        if (!createMemberResult.IsOk)
        {
            Error<ErrorKind> error = createMemberResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("org id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(createMemberResult.Unwrap());
    }

    [Route("api/[controller]/{id}")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read([FromRoute] string id)
    {
        Result<Member, Error<ErrorKind>> readResult = await _memberManager.Read(id);

        if (!readResult.IsOk)
        {
            Error<ErrorKind> error = readResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("member id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return Ok(JsonConvert.SerializeObject(readResult.Unwrap()));
    }

    [Route("api/[controller]")]
    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] Member idMember)
    {
        Result<Empty, Error<ErrorKind>> updateResult = await _memberManager.Update(idMember);

        if (!updateResult.IsOk)
        {
            Error<ErrorKind> error = updateResult.UnwrapErr();

            if (error.ErrorKind == ErrorKind.NotFound) return BadRequest("member id not found");

            return StatusCode(StatusCodes.Status500InternalServerError, error.Message);
        }

        return NoContent();
    }
}