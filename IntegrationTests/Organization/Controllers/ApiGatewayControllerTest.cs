using System.Net;
using System.Net.Http.Json;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.IntegrationTests.Organization.Controllers;

[Collection("Database")]
public class ApiGatewayControllerTest : TestBase
{
    public ApiGatewayControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory,
        output)
    {
    }

    [Fact]
    public async Task SignUpCreatingOrg_ValidData_ReturnsOrgId()
    {
        const int expectedOrgIdLength = 24;
        RegisterCreatingOrgPayload payload = new()
        {
            UserId = "1234",
            Org = new PartialOrganization("example",
                new Address("ES", "Albacete", "Villarrobledo", "Calle", "85", "", "02600"), Array.Empty<string>())
        };


        HttpResponseMessage response =
            await Client.PostAsync("/api/ApiGateway/register-creating-org", JsonContent.Create(payload));


        string content = await response.Content.ReadAsStringAsync();
        Output.WriteLine($"content: {content}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedOrgIdLength, content.Length);
    }
}