using Core;
using Cuplan.Organization.Models;
using Cuplan.Organization.Services;
using Cuplan.Organization.Utils;

namespace Cuplan.Authentication.Models;

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

    public async Task<Result<LoginSuccessPayload, Error<string>>> Register(
        SignUpPayload payload)
    {
        if (!Validation.IsEmailValid(payload.Email))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'email' is invalid."));

        if (!Validation.IsPasswordValid(payload.Password))
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'password' is invalid."));

        Result<string, Error<string>> signUpResult = await _authProvider.SignUp(payload);

        if (!signUpResult.IsOk) return Result<LoginSuccessPayload, Error<string>>.Err(signUpResult.UnwrapErr());
        
        LoginPayload loginPayload = new()
        {
            Email = payload.Email,
            Password = payload.Password
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

    public async Task<Result<LoginSuccessPayload, Error<string>>> RefreshToken(string refreshToken)
    {
        if (refreshToken.Length == 0)
            return Result<LoginSuccessPayload, Error<string>>.Err(new Error<string>(ErrorKind.InvalidCredentials,
                "'refreshToken' is empty."));

        Result<LoginSuccessPayload, Error<string>> result = await _authProvider.RefreshToken(refreshToken);

        return result;
    }
}