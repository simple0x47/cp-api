namespace Cuplan.Organization.Models;

public class Organization
{
    public Organization(string id, string name, Address address, IEnumerable<string> permissions)
    {
        Id = id;
        Name = name;
        Address = address;
        Permissions = permissions;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public IEnumerable<string> Permissions { get; set; }

    public static explicit operator PartialOrganization(Organization org)
    {
        return new PartialOrganization(org.Name, org.Address, org.Permissions);
    }
}