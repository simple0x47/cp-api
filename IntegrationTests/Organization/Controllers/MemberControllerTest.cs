using System.Net;
using System.Net.Http.Json;
using Cuplan.Organization.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.IntegrationTests.Organization.Controllers;

[Collection("Database")]
public class MemberControllerTest : TestBase
{
    private const string OrganizationApi = "api/Organization";
    private const string MemberApi = "api/Member";
    private const string DefaultTestUserId = "example@domain.com";

    public MemberControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(factory,
        output)
    {
    }

    [Fact]
    public async Task CreateMember_WithNonExistingOrgId_Fails()
    {
        PartialMember examplePartialMember =
            new("653bf78afc1ba1ad481195c4", "example@domain.com", Array.Empty<string>(), Array.Empty<string>());
        HttpResponseMessage response = await Client.PostAsync(MemberApi, JsonContent.Create(examplePartialMember));


        string failure = await response.Content.ReadAsStringAsync();


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("org id not found", failure);
    }

    [Fact]
    public async Task CreateMember_WithExistingOrgId_Succeeds()
    {
        const int expectedMemberIdLength = 24;

        string orgId = await CreateOrganization();


        string memberId = await CreateMember(orgId);


        Assert.Equal(expectedMemberIdLength, memberId.Length);
    }

    [Fact]
    public async Task UpdateMember_WithExistingUserId_Succeeds()
    {
        string orgId = await CreateOrganization();
        string memberId = await CreateMember(orgId);

        IEnumerable<string> permissions = new[] { "permission1", "permission2" };
        IEnumerable<string> roles = new[] { "role1", "role2" };
        PartialMember partialMember = new(orgId, DefaultTestUserId, permissions, roles);
        Member idMember = new(memberId, partialMember);


        HttpResponseMessage updateResponse = await Client.PatchAsync(MemberApi, JsonContent.Create(idMember));


        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);


        HttpResponseMessage getResponse = await Client.GetAsync($"{MemberApi}/{memberId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        Member? getMember = await getResponse.Content.ReadFromJsonAsync<Member>();

        Assert.NotNull(getMember);
        Assert.True(getMember.Permissions.SequenceEqual(permissions));
        Assert.True(getMember.Roles.SequenceEqual(roles));
    }

    private async Task<string> CreateOrganization()
    {
        PartialOrganization exampleOrg =
            new("a", new Address("a", "b", "c", "d", "e", "f", "g"),
                new[] { "a" });
        HttpResponseMessage orgResponse = await Client.PostAsync(OrganizationApi, JsonContent.Create(exampleOrg));


        Assert.Equal(HttpStatusCode.OK, orgResponse.StatusCode);


        return await orgResponse.Content.ReadAsStringAsync();
    }

    private async Task<string> CreateMember(string orgId)
    {
        PartialMember examplePartialMember =
            new(orgId, DefaultTestUserId, Array.Empty<string>(), Array.Empty<string>());
        HttpResponseMessage response = await Client.PostAsync(MemberApi, JsonContent.Create(examplePartialMember));


        string memberId = await response.Content.ReadAsStringAsync();


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return memberId;
    }
}