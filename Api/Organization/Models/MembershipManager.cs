using Core;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Models;

public class MembershipManager
{
    private readonly IMemberRepository _memberRepository;
    private readonly OrganizationManager _orgManager;
    private readonly IOrganizationRepository _orgRepository;
    private readonly RoleManager _roleManager;

    public MembershipManager(IMemberRepository memberRepository, IOrganizationRepository orgRepository,
        OrganizationManager orgManager, RoleManager roleManager)
    {
        _memberRepository = memberRepository;
        _orgRepository = orgRepository;
        _orgManager = orgManager;
        _roleManager = roleManager;
    }

    /// <summary>
    ///     Creates the member.
    /// </summary>
    /// <returns>Id of the created member.</returns>
    public async Task<Result<string, Error<string>>> Create(PartialMembership partialMembership)
    {
        Result<Organization, Error<string>> findOrgResult = await _orgRepository.FindById(partialMembership.OrgId);

        if (!findOrgResult.IsOk) return Result<string, Error<string>>.Err(findOrgResult.UnwrapErr());

        Result<string, Error<string>> createMemberResult = await _memberRepository.Create(partialMembership);

        if (!createMemberResult.IsOk) return Result<string, Error<string>>.Err(createMemberResult.UnwrapErr());

        string memberId = createMemberResult.Unwrap();

        return Result<string, Error<string>>.Ok(memberId);
    }

    /// <summary>
    ///     Reads a membership by its id.
    /// </summary>
    /// <param name="memberId"></param>
    /// <returns>A <see cref="Membership" /> or an error.</returns>
    public async Task<Result<Membership, Error<string>>> Read(string memberId)
    {
        Result<Membership, Error<string>> findMemberResult = await _memberRepository.FindById(memberId);

        if (!findMemberResult.IsOk)
            return Result<Membership, Error<string>>.Err(findMemberResult.UnwrapErr());

        Membership idMembership = findMemberResult.Unwrap();

        return Result<Membership, Error<string>>.Ok(idMembership);
    }

    /// <summary>
    ///     Updates the member.
    /// </summary>
    /// <param name="idMembership">The updated member.</param>
    /// <returns>An empty result indicating the operation was successful, or an error.</returns>
    public async Task<Result<Empty, Error<string>>> Update(Membership idMembership)
    {
        Result<Empty, Error<string>> updatePermissions =
            await _memberRepository.SetPermissions(idMembership.Id, idMembership.Permissions);

        if (!updatePermissions.IsOk) return Result<Empty, Error<string>>.Err(updatePermissions.UnwrapErr());

        IList<string> roleIds = new List<string>();

        foreach (Role role in idMembership.Roles) roleIds.Add(role.Id);

        Result<Empty, Error<string>> updateRoles = await _memberRepository.SetRoles(idMembership.Id, roleIds);

        if (!updateRoles.IsOk) return Result<Empty, Error<string>>.Err(updateRoles.UnwrapErr());

        return Result<Empty, Error<string>>.Ok(new Empty());
    }

    /// <summary>
    ///     Retrieves all the memberships an user has.
    /// </summary>
    /// <param name="userId">Id of the user whose memberships must be retrieved.</param>
    /// <returns>An array of <see cref="Membership" /> containing all the memberships of the user or an error.</returns>
    public async Task<Result<Membership[], Error<string>>> ReadMembersByUserId(string userId)
    {
        Result<Membership[], Error<string>> result = await _memberRepository.FindByUserId(userId);

        if (!result.IsOk) return result;

        Membership[] memberships = result.Unwrap();

        foreach (Membership membership in memberships)
        {
            Result<Organization, Error<string>> orgResult = await _orgRepository.FindById(membership.OrgId);

            if (!orgResult.IsOk) return Result<Membership[], Error<string>>.Err(orgResult.UnwrapErr());

            membership.OrgName = orgResult.Unwrap().Name;
        }

        return result;
    }

    public async Task<Result<string, Error<string>>> UserCreateOrg(UserCreateOrgPayload payload)
    {
        Result<string, Error<string>> createOrgResult = await _orgManager.Create(payload.Org);

        if (!createOrgResult.IsOk)
            return Result<string, Error<string>>.Err(createOrgResult.UnwrapErr());

        string orgId = createOrgResult.Unwrap();

        Result<Role, Error<string>> adminRoleResult = await _roleManager.GetAdminRole();

        if (!adminRoleResult.IsOk)
            return Result<string, Error<string>>.Err(adminRoleResult.UnwrapErr());

        Role adminRole = adminRoleResult.Unwrap();

        PartialMembership partialMembership = new(orgId, payload.UserId, Array.Empty<string>(), new[] { adminRole });

        Result<string, Error<string>> createMemberResult = await Create(partialMembership);

        if (!createMemberResult.IsOk) return Result<string, Error<string>>.Err(createMemberResult.UnwrapErr());

        return Result<string, Error<string>>.Ok(createMemberResult.Unwrap());
    }
}