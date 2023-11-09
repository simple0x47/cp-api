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
    public async Task RegisterCreatingOrg_ValidData_ReturnsOrgId()
    {
        HttpResponseMessage response = await RegisterCreatingOrg($"{Guid.NewGuid().ToString()}@simpleg.eu");


        await AssertSuccessfulLogin(response);
    }

    [Fact]
    public async Task RegisterCreatingOrg_AlreadyRegisteredEmail_Fails()
    {
        HttpResponseMessage response =
            await RegisterCreatingOrg(SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginEmailSecretVar)));

        string content = await response.Content.ReadAsStringAsync();


        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("invalid_signup", content);
    }

    [Fact]
    public async Task Login_PreviouslyCreatedUser_Succeeds()
    {
        string? email = SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginEmailSecretVar));
        string? password = SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginPasswordSecretVar));

        Assert.NotNull(email);
        Assert.NotNull(password);

        LoginPayload payload = new();
        payload.Email = email;
        payload.Password = password;


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/login", JsonContent.Create(payload));


        await AssertSuccessfulLogin(response);
    }

    [Fact]
    public async Task ForgotPassword_NonExistingUser_Succeeds()
    {
        ForgotPasswordPayload payload = new()
        {
            Email = $"{Guid.NewGuid().ToString()}@simpleg.eu"
        };


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/forgot-password", JsonContent.Create(payload));


        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_Succeeds()
    {
        ForgotPasswordPayload payload = new()
        {
            Email = SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginEmailSecretVar))
        };


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/forgot-password", JsonContent.Create(payload));


        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_PreviouslyLoggedIn_Succeeds()
    {
        string? email = SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginEmailSecretVar));
        string? password = SecretsManager.Get(Environment.GetEnvironmentVariable(TestLoginPasswordSecretVar));

        Assert.NotNull(email);
        Assert.NotNull(password);

        LoginPayload payload = new();
        payload.Email = email;
        payload.Password = password;

        HttpResponseMessage loginResponse =
            await Client.PostAsync($"{AuthenticationApi}/login", JsonContent.Create(payload));
        LoginSuccessPayload successPayload = await loginResponse.Content.ReadFromJsonAsync<LoginSuccessPayload>();


        HttpResponseMessage response = await Client.PostAsync($"{AuthenticationApi}/refresh-token",
            JsonContent.Create(successPayload.RefreshToken));


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertSuccessfulLogin(response);
    }

    private async Task<HttpResponseMessage> RegisterCreatingOrg(string email)
    {
        SignUpPayload signUp = new();
        signUp.FullName = "Integration Test";
        signUp.Email = email;
        signUp.Password = Guid.NewGuid().ToString();

        RegisterCreatingOrgPayload payload = new()
        {
            User = signUp,
            Org = new PartialOrganization("example",
                new Address("ES", "Albacete", "Villarrobledo", "Calle", "85", "", "02600"), Array.Empty<string>())
        };


        HttpResponseMessage response =
            await Client.PostAsync($"{AuthenticationApi}/register-creating-org", JsonContent.Create(payload));

        return response;
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