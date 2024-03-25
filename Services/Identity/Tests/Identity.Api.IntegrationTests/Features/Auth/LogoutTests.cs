using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Api.IntegrationTests.Features.Auth;

public class LogoutTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "logoutEndpoint");

    [Fact]
    public async Task Logout_withValidRefreshToken_ReturnsNoContentResult()
    {
        // arrange
        var createModel = new RegisterRequest("test@email.com", "strongPwd!1");
        await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(createModel));
        var loginResponse = await TestCase.Login(_client, createModel.Email, createModel.Password);

        var model = new RefreshRequest(loginResponse.RefreshToken);

        // act
        var response = await _client.PostAsync("api/v1/auth/logout", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_withInvalidRefreshToken_ReturnsUnauthorizedResult()
    {
        // arrange
        var model = new RefreshRequest(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));

        // act
        var response = await _client.PostAsync("api/v1/auth/logout", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
}