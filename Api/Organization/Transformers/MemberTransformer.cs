using Core;
using Cuplan.Organization.Models;
using Cuplan.Organization.Services;

namespace Cuplan.Organization.Transformers;

/// <summary>
///     Transformer of <see cref="ServiceModels.Membership" /> into <see cref="Membership" />.
/// </summary>
public static class MemberTransformer
{
    public static async Task<Result<Membership, Error<string>>> Transform(this ServiceModels.Membership membership,
        IRoleRepository roleRepository)
    {
        Result<IEnumerable<Role>, Error<string>> rolesResult = await roleRepository.FindByIds(membership.Roles);

        if (!rolesResult.IsOk) return Result<Membership, Error<string>>.Err(rolesResult.UnwrapErr());

        IEnumerable<Role> roles = rolesResult.Unwrap();

        Membership modelsMembership = new()
        {
            Id = membership.Id.ToString(),
            OrgId = membership.OrgId,
            Permissions = membership.Permissions,
            Roles = roles,
            UserId = membership.UserId
        };

        return Result<Membership, Error<string>>.Ok(modelsMembership);
    }
}