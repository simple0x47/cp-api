using Cuplan.Organization.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cuplan.Organization.ServiceModels;

public class Member
{
    public Member()
    {
    }

    public Member(PartialMember partialMember)
    {
        OrgId = partialMember.OrgId;
        UserId = partialMember.UserId;
        Permissions = partialMember.Permissions;
        Roles = partialMember.Roles;
    }

    public Member(ObjectId id, string orgId, string userId, IEnumerable<string> permissions,
        IEnumerable<string> roles)
    {
        Id = id;
        OrgId = orgId;
        UserId = userId;
        Permissions = permissions;
        Roles = roles;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string OrgId { get; set; }
    public string UserId { get; set; }
    public IEnumerable<string> Permissions { get; set; }
    public IEnumerable<string> Roles { get; set; }

    public static implicit operator Models.Member(Member member)
    {
        return new Models.Member(member.Id.ToString(), member.OrgId, member.UserId, member.Permissions, member.Roles);
    }
}