using Core;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Models;

public class MemberManager
{
    private readonly IMemberRepository _memberRepository;
    private readonly IOrganizationRepository _orgRepository;

    public MemberManager(IMemberRepository memberRepository, IOrganizationRepository orgRepository)
    {
        _memberRepository = memberRepository;
        _orgRepository = orgRepository;
    }

    /// <summary>
    ///     Creates the member.
    /// </summary>
    /// <returns>Id of the created member.</returns>
    public async Task<Result<string, Error<string>>> Create(PartialMember partialMember)
    {
        Result<Organization, Error<string>> findOrgResult = await _orgRepository.FindById(partialMember.OrgId);

        if (!findOrgResult.IsOk) return Result<string, Error<string>>.Err(findOrgResult.UnwrapErr());

        Result<string, Error<string>> createMemberResult = await _memberRepository.Create(partialMember);

        if (!createMemberResult.IsOk) return Result<string, Error<string>>.Err(createMemberResult.UnwrapErr());

        string memberId = createMemberResult.Unwrap();

        return Result<string, Error<string>>.Ok(memberId);
    }

    /// <summary>
    ///     Reads a membership by its id.
    /// </summary>
    /// <param name="memberId"></param>
    /// <returns>A <see cref="Member" /> or an error.</returns>
    public async Task<Result<Member, Error<string>>> Read(string memberId)
    {
        Result<Member, Error<string>> findMemberResult = await _memberRepository.FindById(memberId);

        if (!findMemberResult.IsOk)
            return Result<Member, Error<string>>.Err(findMemberResult.UnwrapErr());

        Member idMember = findMemberResult.Unwrap();

        return Result<Member, Error<string>>.Ok(idMember);
    }

    /// <summary>
    ///     Updates the member.
    /// </summary>
    /// <param name="idMember">The updated member.</param>
    /// <returns>An empty result indicating the operation was successful, or an error.</returns>
    public async Task<Result<Empty, Error<string>>> Update(Member idMember)
    {
        Result<Empty, Error<string>> updatePermissions =
            await _memberRepository.SetPermissions(idMember.Id, idMember.Permissions);

        if (!updatePermissions.IsOk) return Result<Empty, Error<string>>.Err(updatePermissions.UnwrapErr());

        IList<string> roleIds = new List<string>();

        foreach (Role role in idMember.Roles) roleIds.Add(role.Id);

        Result<Empty, Error<string>> updateRoles = await _memberRepository.SetRoles(idMember.Id, roleIds);

        if (!updateRoles.IsOk) return Result<Empty, Error<string>>.Err(updateRoles.UnwrapErr());

        return Result<Empty, Error<string>>.Ok(new Empty());
    }

    /// <summary>
    ///     Retrieves all the memberships an user has.
    /// </summary>
    /// <param name="userId">Id of the user whose memberships must be retrieved.</param>
    /// <returns>An array of <see cref="Member" /> containing all the memberships of the user or an error.</returns>
    public async Task<Result<Member[], Error<string>>> ReadMembersByUserId(string userId)
    {
        Result<Member[], Error<string>> result = await _memberRepository.FindByUserId(userId);

        if (!result.IsOk) return result;

        Member[] memberships = result.Unwrap();

        foreach (Member membership in memberships)
        {
            Result<Organization, Error<string>> orgResult = await _orgRepository.FindById(membership.OrgId);

            if (!orgResult.IsOk) return Result<Member[], Error<string>>.Err(orgResult.UnwrapErr());

            membership.OrgName = orgResult.Unwrap().Name;
        }

        return result;
    }
}