using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using FluentAssertions;
using IdentityApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Api.IntegrationTests.Features.Auth;

public class RefreshTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "refreshEndpoint");

    [Fact]
    public async Task Refresh_withValidRefreshToken_ReturnsOkResult()
    {
        // arrange
        var createModel = new RegisterRequest("test@email.com", "strongPwd!1");
        await _client.PostAsJsonAsync("api/v1/auth/register", createModel);
        var loginResponse = await TestCase.Login(_client, createModel.Email, createModel.Password);
        var refreshModel = new RefreshRequest(loginResponse.RefreshToken);

        // act
        var response = await _client.PostAsJsonAsync("api/v1/auth/refresh", refreshModel);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_withInvalidRefreshToken_ReturnsUnauthorizedResult()
    {
        // arrange
        var model = new RefreshRequest(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));

        // act
        var response = await _client.PostAsJsonAsync("api/v1/auth/refresh", model);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}