namespace Cuplan.Organization.Models;

public class Address
{
    public Address(string country, string province, string city, string street, string number, string? additional,
        string postalCode)
    {
        Country = country;
        Province = province;
        City = city;
        Street = street;
        Number = number;
        Additional = additional;
        PostalCode = postalCode;
    }

    public string Country { get; }
    public string Province { get; }
    public string City { get; }
    public string Street { get; }
    public string Number { get; }
    public string? Additional { get; }
    public string PostalCode { get; }
}