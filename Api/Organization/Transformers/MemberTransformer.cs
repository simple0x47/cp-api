using Core;
using Cuplan.Organization.Models;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Transformers;

/// <summary>
///     Transformer of <see cref="ServiceModels.Member" /> into <see cref="Member" />.
/// </summary>
public static class MemberTransformer
{
    public static async Task<Result<Member, Error<string>>> Transform(this ServiceModels.Member member,
        IRoleRepository roleRepository)
    {
        Result<IEnumerable<Role>, Error<string>> rolesResult = await roleRepository.FindByIds(member.Roles);

        if (!rolesResult.IsOk) return Result<Member, Error<string>>.Err(rolesResult.UnwrapErr());

        IEnumerable<Role> roles = rolesResult.Unwrap();

        Member modelsMember = new()
        {
            Id = member.Id.ToString(),
            OrgId = member.OrgId,
            Permissions = member.Permissions,
            Roles = roles,
            UserId = member.UserId
        };

        return Result<Member, Error<string>>.Ok(modelsMember);
    }
}