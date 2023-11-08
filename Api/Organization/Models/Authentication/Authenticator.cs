using Core;
using Cuplan.Organization.Services;
using Cuplan.Organization.Utils;

namespace Cuplan.Organization.Models.Authentication;

public class Authenticator
{
    private readonly IAuthProvider _authProvider;
    private readonly MembershipManager _membershipManager;
    private readonly OrganizationManager _orgManager;
    private readonly RoleManager _roleManager;

    public Authenticator(OrganizationManager orgManager, MembershipManager membershipManager, RoleManager roleManager,
        IAuthProvider authProvider)
    {
        _orgManager = orgManager;
        _membershipManager = membershipManager;
        _roleManager = roleManager;
        _authProvider = authProvider;
    }

    public async Task<Result<LoginSuccessPayload, Error<string>>> RegisterCreatingOrg(
        RegisterCreatingOrgPayload payload)
    {
        if (!Validation.IsEmailValid(payload.User.Email))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'email' is invalid."));

        if (!Validation.IsPasswordValid(payload.User.Password))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'password' is invalid."));

        Result<string, Error<string>> signUpResult = await _authProvider.SignUp(payload.User);

        if (!signUpResult.IsOk) return Result<LoginSuccessPayload, Error<string>>.Err(signUpResult.UnwrapErr());

        string userId = signUpResult.Unwrap();

        Result<string, Error<string>> createOrgResult = await _orgManager.Create(payload.Org);

        if (!createOrgResult.IsOk)
            return Result<LoginSuccessPayload, Error<string>>.Err(createOrgResult.UnwrapErr());

        string orgId = createOrgResult.Unwrap();

        Result<Role, Error<string>> adminRoleResult = await _roleManager.GetAdminRole();

        if (!adminRoleResult.IsOk)
            return Result<LoginSuccessPayload, Error<string>>.Err(adminRoleResult.UnwrapErr());

        Role adminRole = adminRoleResult.Unwrap();

        PartialMembership partialMembership = new(orgId, userId, Array.Empty<string>(), new[] { adminRole });

        Result<string, Error<string>> createMemberResult = await _membershipManager.Create(partialMembership);

        if (!createMemberResult.IsOk)
            return Result<LoginSuccessPayload, Error<string>>.Err(createMemberResult.UnwrapErr());

        LoginPayload loginPayload = new()
        {
            Email = payload.User.Email,
            Password = payload.User.Password
        };

        return await Login(loginPayload);
    }

    public async Task<Result<LoginSuccessPayload, Error<string>>> Login(LoginPayload payload)
    {
        if (!Validation.IsEmailValid(payload.Email))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'email' is invalid."));

        if (!Validation.IsPasswordValid(payload.Password))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'password' is invalid."));

        Result<LoginSuccessPayload, Error<string>> result = await _authProvider.Login(payload);

        return result;
    }

    public async Task<Result<Empty, Error<string>>> ForgotPassword(ForgotPasswordPayload payload)
    {
        if (!Validation.IsEmailValid(payload.Email))
            return Result<Empty, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'email' is invalid."));

        Result<Empty, Error<string>> result = await _authProvider.ForgotPassword(payload);

        return result;
    }
}