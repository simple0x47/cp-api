using Core;
using Cuplan.Organization.Services;
using Cuplan.Organization.Utils;

namespace Cuplan.Organization.Models.Authentication;

public class Authenticator
{
    private readonly IAuthProvider _authProvider;
    private readonly MemberManager _memberManager;
    private readonly OrganizationManager _orgManager;
    private readonly RoleManager _roleManager;

    public Authenticator(OrganizationManager orgManager, MemberManager memberManager, RoleManager roleManager,
        IAuthProvider authProvider)
    {
        _orgManager = orgManager;
        _memberManager = memberManager;
        _roleManager = roleManager;
        _authProvider = authProvider;
    }

    public async Task<Result<LoginSuccessPayload, Error<ErrorKind>>> RegisterCreatingOrg(
        RegisterCreatingOrgPayload payload)
    {
        if (!Validation.IsEmailValid(payload.User.Email))
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.InvalidData,
                "'email' is invalid."));

        if (!Validation.IsPasswordValid(payload.User.Password))
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.InvalidData,
                "'password' is invalid."));

        Result<string, Error<ErrorKind>> signUpResult = await _authProvider.SignUp(payload.User);

        if (!signUpResult.IsOk) return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(signUpResult.UnwrapErr());

        string userId = signUpResult.Unwrap();

        Result<string, Error<ErrorKind>> createOrgResult = await _orgManager.Create(payload.Org);

        if (!createOrgResult.IsOk)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(createOrgResult.UnwrapErr());

        string orgId = createOrgResult.Unwrap();

        Result<Role, Error<ErrorKind>> adminRoleResult = await _roleManager.GetAdminRole();

        if (!adminRoleResult.IsOk)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(adminRoleResult.UnwrapErr());

        Role adminRole = adminRoleResult.Unwrap();

        PartialMember partialMember = new(orgId, userId, Array.Empty<string>(), new[] { adminRole });

        Result<string, Error<ErrorKind>> createMemberResult = await _memberManager.Create(partialMember);

        if (!createMemberResult.IsOk)
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(createMemberResult.UnwrapErr());

        LoginPayload loginPayload = new()
        {
            Email = payload.User.Email,
            Password = payload.User.Password
        };

        return await Login(loginPayload);
    }

    public async Task<Result<LoginSuccessPayload, Error<ErrorKind>>> Login(LoginPayload payload)
    {
        if (!Validation.IsEmailValid(payload.Email))
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.InvalidData,
                "'email' is invalid."));

        if (!Validation.IsPasswordValid(payload.Password))
            return Result<LoginSuccessPayload, Error<ErrorKind>>.Err(new Error<ErrorKind>(ErrorKind.InvalidData,
                "'password' is invalid."));

        Result<LoginSuccessPayload, Error<ErrorKind>> result = await _authProvider.Login(payload);

        return result;
    }
}