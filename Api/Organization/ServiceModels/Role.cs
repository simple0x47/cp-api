using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cuplan.Organization.ServiceModels;

public class Role
{
    public Role(ObjectId id, string name, IEnumerable<string> permissions)
    {
        Id = id;
        Name = name;
        Permissions = permissions;
        DefaultAdmin = null;
        DefaultMember = null;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string Name { get; set; }
    public IEnumerable<string> Permissions { get; set; }
    public bool? DefaultAdmin { get; set; }
    public bool? DefaultMember { get; set; }

    public static implicit operator Models.Role(Role role)
    {
        return new Models.Role(role.Id.ToString(), role.Name, role.Permissions);
    }
}