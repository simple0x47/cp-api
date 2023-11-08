using Cuplan.Organization.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cuplan.Organization.ServiceModels;

public class Membership
{
    public Membership()
    {
    }

    public Membership(PartialMembership partialMembership)
    {
        OrgId = partialMembership.OrgId;
        UserId = partialMembership.UserId;
        Permissions = partialMembership.Permissions;

        IList<string> roleIds = new List<string>();

        foreach (Models.Role role in partialMembership.Roles) roleIds.Add(role.Id);

        Roles = roleIds;
    }

    public Membership(ObjectId id, string orgId, string userId, IEnumerable<string> permissions,
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