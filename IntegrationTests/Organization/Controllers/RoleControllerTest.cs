using System.Net;
using System.Net.Http.Json;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.IntegrationTests.Organization.Controllers;

[Collection("Database")]
public class RoleControllerTest : TestBase
{
    private const string RoleApi = "api/Role";

    public RoleControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory, output)
    {
    }

    [Fact]
    public async Task GetAdminRole_Succeeds()
    {
        HttpResponseMessage response = await Client.GetAsync($"{RoleApi}/AdminRole");


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Role? adminRole = await response.Content.ReadFromJsonAsync<Role>();

        Assert.NotNull(adminRole);
    }
}