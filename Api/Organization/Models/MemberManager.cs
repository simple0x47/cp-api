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
    public async Task<Result<string, Error<ErrorKind>>> Create(PartialMember partialMember)
    {
        Result<Organization, Error<ErrorKind>> findOrgResult = await _orgRepository.FindById(partialMember.OrgId);

        if (!findOrgResult.IsOk) return Result<string, Error<ErrorKind>>.Err(findOrgResult.UnwrapErr());

        Result<string, Error<ErrorKind>> createMemberResult = await _memberRepository.Create(partialMember);

        if (!createMemberResult.IsOk) return Result<string, Error<ErrorKind>>.Err(createMemberResult.UnwrapErr());

        string memberId = createMemberResult.Unwrap();

        return Result<string, Error<ErrorKind>>.Ok(memberId);
    }

    /// <summary>
    ///     Reads a membership by its id.
    /// </summary>
    /// <param name="memberId"></param>
    /// <returns>A <see cref="Member" /> or an error.</returns>
    public async Task<Result<Member, Error<ErrorKind>>> Read(string memberId)
    {
        Result<Member, Error<ErrorKind>> findMemberResult = await _memberRepository.FindById(memberId);

        if (!findMemberResult.IsOk)
            return Result<Member, Error<ErrorKind>>.Err(findMemberResult.UnwrapErr());

        Member idMember = findMemberResult.Unwrap();

        return Result<Member, Error<ErrorKind>>.Ok(idMember);
    }

    /// <summary>
    ///     Updates the member.
    /// </summary>
    /// <param name="idMember">The updated member.</param>
    /// <returns>An empty result indicating the operation was successful, or an error.</returns>
    public async Task<Result<Empty, Error<ErrorKind>>> Update(Member idMember)
    {
        Result<Empty, Error<ErrorKind>> updatePermissions =
            await _memberRepository.SetPermissions(idMember.Id, idMember.Permissions);

        if (!updatePermissions.IsOk) return Result<Empty, Error<ErrorKind>>.Err(updatePermissions.UnwrapErr());

        Result<Empty, Error<ErrorKind>> updateRoles = await _memberRepository.SetRoles(idMember.Id, idMember.Roles);

        if (!updateRoles.IsOk) return Result<Empty, Error<ErrorKind>>.Err(updateRoles.UnwrapErr());

        return Result<Empty, Error<ErrorKind>>.Ok(new Empty());
    }

    /// <summary>
    ///     Retrieves all the memberships an user has.
    /// </summary>
    /// <param name="userId">Id of the user whose memberships must be retrieved.</param>
    /// <returns>An array of <see cref="Member" /> containing all the memberships of the user or an error.</returns>
    public async Task<Result<Member[], Error<ErrorKind>>> ReadMembersByUserId(string userId)
    {
        Result<Member[], Error<ErrorKind>> result = await _memberRepository.FindByUserId(userId);

        return result;
    }
}