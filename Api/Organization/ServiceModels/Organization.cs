using Cuplan.Organization.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cuplan.Organization.ServiceModels;

public class Organization
{
    public Organization()
    {
    }

    public Organization(PartialOrganization partialOrg)
    {
        Name = partialOrg.Name;
        Address = partialOrg.Address;
        Permissions = partialOrg.Permissions;
    }

    public Organization(ObjectId id, string name, Address address, IEnumerable<string> permissions)
    {
        Id = id;
        Name = name;
        Address = address;
        Permissions = permissions;
    }

    [BsonId] public ObjectId Id { get; set; }

    public string Name { get; set; }
    public Address Address { get; set; }
    public IEnumerable<string> Permissions { get; set; }

    public static implicit operator Models.Organization(Organization org)
    {
        return new Models.Organization(org.Id.ToString(), org.Name, org.Address, org.Permissions);
    }
}