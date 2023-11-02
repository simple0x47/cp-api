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

        IList<string> roleIds = new List<string>();

        foreach (Models.Role role in partialMember.Roles) roleIds.Add(role.Id);

        Roles = roleIds;
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
}