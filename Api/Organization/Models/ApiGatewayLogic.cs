using Core;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Models;

public class ApiGatewayLogic
{
    private readonly IAuthProvider _authProvider;
    private readonly MemberManager _memberManager;
    private readonly OrganizationManager _orgManager;
    private readonly RoleManager _roleManager;

    public ApiGatewayLogic(OrganizationManager orgManager, MemberManager memberManager, RoleManager roleManager,
        IAuthProvider authProvider)
    {
        _orgManager = orgManager;
        _memberManager = memberManager;
        _roleManager = roleManager;
        _authProvider = authProvider;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public async Task<Result<string, Error<ErrorKind>>> RegisterCreatingOrg(RegisterCreatingOrgPayload payload)
    {
        Result<string, Error<ErrorKind>> signUpResult = await _authProvider.SignUp(payload.User);

        if (!signUpResult.IsOk) return Result<string, Error<ErrorKind>>.Err(signUpResult.UnwrapErr());

        string userId = signUpResult.Unwrap();

        Result<string, Error<ErrorKind>> createOrgResult = await _orgManager.Create(payload.Org);

        if (!createOrgResult.IsOk) return Result<string, Error<ErrorKind>>.Err(createOrgResult.UnwrapErr());

        string orgId = createOrgResult.Unwrap();

        Result<Role, Error<ErrorKind>> adminRoleResult = await _roleManager.GetAdminRole();

        if (!adminRoleResult.IsOk) return Result<string, Error<ErrorKind>>.Err(adminRoleResult.UnwrapErr());

        Role adminRole = adminRoleResult.Unwrap();

        PartialMember partialMember = new(orgId, userId, Array.Empty<string>(), new[] { adminRole.Id });

        Result<string, Error<ErrorKind>> createMemberResult = await _memberManager.Create(partialMember);

        if (!createMemberResult.IsOk) return Result<string, Error<ErrorKind>>.Err(createMemberResult.UnwrapErr());

        string memberId = createMemberResult.Unwrap();

        return Result<string, Error<ErrorKind>>.Ok(memberId);
    }
}