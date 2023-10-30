using System.Net;
using System.Net.Http.Json;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.IntegrationTests.Organization.Controllers;

[Collection("Database")]
public class OrganizationControllerTest : TestBase
{
    public OrganizationControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory,
        output)
    {
    }

    [Fact]
    public async Task CreateOrganization_ReturnsAnOrganizationId()
    {
        const int organizationIdLength = 24;

        PartialOrganization examplePartialOrganization =
            new("a", new Address("a", "b", "c", "d", "e", "f", "g"),
                new[] { "a" });
        HttpResponseMessage response =
            await Client.PostAsync("api/Organization", JsonContent.Create(examplePartialOrganization));


        string organizationId = await response.Content.ReadAsStringAsync();


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(organizationIdLength, organizationId.Length);
    }
}