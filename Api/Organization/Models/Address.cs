namespace Cuplan.Organization.Models;

public class Address(string country, string province, string city, string street, string number, string? additional, string postalCode)
{
    public string Country { get; } = country;
    public string Province { get; } = province;
    public string City { get; } = city;
    public string Street { get; } = street;
    public string Number { get; } = number;
    public string? Additional { get; } = additional;
    public string PostalCode { get; } = postalCode;
}