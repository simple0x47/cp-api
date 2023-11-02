namespace Cuplan.Organization.Models;

public class PartialOrganization
{
    public PartialOrganization(string name, Address address, IEnumerable<string> permissions)
    {
        Name = name;
        Address = address;
        Permissions = permissions;
    }

    public string Name { get; set; }
    public Address Address { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}