namespace Cuplan.Organization.Models;

public class Role
{
    public Role(string id, string name, IEnumerable<string> permissions)
    {
        Id = id;
        Name = name;
        Permissions = permissions;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}