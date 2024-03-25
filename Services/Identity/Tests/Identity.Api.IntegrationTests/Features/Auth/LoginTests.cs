using System.Net;
using FluentAssertions;
using Identity.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using RegisterRequest = Identity.Api.Contracts.RegisterRequest;

namespace Identity.Api.IntegrationTests.Features.Auth;

public class LoginTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "loginEndpoint");
    
    [Fact]
    public async Task Login_withValidUser_ReturnsOkResult()
    {
        // arrange
        var createModel = new RegisterRequest("test@email.com", "strongPwd!1");
        var result = await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(createModel));
        var loginModel = new LoginRequest(createModel.Email, createModel.Password);
        
        // act
        var response = await _client.PostAsync("api/v1/auth/login", TestCase.CreateContext(loginModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Theory]
    [InlineData("test@email.com", "strongPwd!")]
    [InlineData("test2@email.com", "myPassword")]
    public async Task Login_withNonExistentUser_ReturnsNotFoundResult(string email, string password)
    {
        // arrange
        var model = new LoginRequest(email, password);
        
        // act
        var response = await _client.PostAsync("api/v1/auth/login", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}