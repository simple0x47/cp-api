using System.Net;
using System.Net.Http.Json;
using Cuplan.Organization.Models;
using Cuplan.Organization.Models.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.IntegrationTests.Organization.Controllers;

[Collection("Database")]
public class AuthenticationControllerTest : TestBase
{
    private const string TestLoginEmailSecretVar = "TEST_LOGIN_EMAIL_SECRET";
    private const string TestLoginPasswordSecretVar = "TEST_LOGIN_PASSWORD_SECRET";
    private const string AuthenticationApi = "api/Authentication";

    public AuthenticationControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output) : base(
        factory,
        output)
    {
    }

    [Fact]
    public async Task SignUpCreatingOrg_ValidData_ReturnsOrgId()
    {
        SignUpPayload signUp = new();
        signUp.FullName = "Integration Test";
        signUp.Email = $"{Guid.NewGuid().ToString()}@simpleg.eu";
        signUp.Password = Guid.NewGuid().ToString();

        RegisterCreatingOrgPayload payload = new()
        {
            User = signUp,
            Org = new PartialOrganization("example",
                new Address("ES", "Albacete", "Villarrobledo", "Calle", "85", "", "02600"), Array.Empty<string>())
        };


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/register-creating-org", JsonContent.Create(payload));


        AssertSuccessfulLogin(response);
    }

    [Fact]
    public async Task Login_PreviouslyCreatedUser_Succeeds()
    {
        string? email = SecretsManager.get(Environment.GetEnvironmentVariable(TestLoginEmailSecretVar));
        string? password = SecretsManager.get(Environment.GetEnvironmentVariable(TestLoginPasswordSecretVar));

        Assert.NotNull(email);
        Assert.NotNull(password);

        LoginPayload payload = new();
        payload.Email = email;
        payload.Password = password;


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/login", JsonContent.Create(payload));


        AssertSuccessfulLogin(response);
    }

    private async Task AssertSuccessfulLogin(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        Output.WriteLine($"content: {content}");
        LoginSuccessPayload? loginSuccessPayload = JsonConvert.DeserializeObject<LoginSuccessPayload>(content);


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(loginSuccessPayload);
        Assert.True(loginSuccessPayload.Value.AccessToken.Length > 0);
        Assert.True(loginSuccessPayload.Value.IdToken.Length > 0);
        Assert.True(loginSuccessPayload.Value.RefreshToken.Length > 0);
        Assert.True(loginSuccessPayload.Value.ExpiresIn > 0);
    }
}