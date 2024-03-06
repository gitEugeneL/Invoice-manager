using System.Net;
using System.Security.Cryptography;
using FluentAssertions;
using Identity.Api.Models.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Api.IntegrationTests.Endpoints;

public class AuthEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "authEndpoints");

    [Theory]
    [InlineData("mailt@mail.test", "strongPwd!1")]
    [InlineData("mail1@mail.test", "myPassword12@")]
    public async Task Register_withValidBody_ReturnsCreatedResult(string email, string password)
    {
        // arrange
        var model = new CreateUserDto(email, password);

        // act
        var response = await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Fact]
    public async Task Register_withExistingUser_ReturnsConflictResult()
    {
        // arrange
        var model = new CreateUserDto("test@email.com", "strongPwd!1");
        
        // act
        var response  = new HttpResponseMessage();
        for (var i = 0; i < 2; i++)
            response = await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    
    [Fact]
    public async Task Login_withValidUser_ReturnsOkResult()
    {
        // arrange
        var createModel = new CreateUserDto("test@email.com", "strongPwd!1");
        await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(createModel));
        var loginModel = new LoginDto(createModel.Email, createModel.Password);
        
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
        var model = new LoginDto(email, password);
        
        // act
        var response = await _client.PostAsync("api/v1/auth/login", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Refresh_withValidRefreshToken_ReturnsOkResult()
    {
        // arrange
        var createModel = new CreateUserDto("test@email.com", "strongPwd!1");
        await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(createModel));
        var loginResponse = await TestCase.Login(_client, createModel.Email, createModel.Password);
        var refreshModel = new RefreshDto(loginResponse.RefreshToken);
        
        // act
        var response = await _client.PostAsync("api/v1/auth/refresh", TestCase.CreateContext(refreshModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Refresh_withInvalidRefreshToken_ReturnsUnauthorizedResult()
    {
        // arrange
        var model = new RefreshDto(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));
        
        // act
        var response = await _client.PostAsync("api/v1/auth/refresh", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Logout_withValidRefreshToken_ReturnsNoContentResult()
    {
        // arrange
        var createModel = new CreateUserDto("test@email.com", "strongPwd!1");
        await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(createModel));
        var loginResponse = await TestCase.Login(_client, createModel.Email, createModel.Password);
        
        var model = new RefreshDto(loginResponse.RefreshToken);
        
        // act
        var response = await _client.PostAsync("api/v1/auth/logout", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task Logout_withInvalidRefreshToken_ReturnsUnauthorizedResult()
    {
        // arrange
        var model = new RefreshDto(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));
        
        // act
        var response = await _client.PostAsync("api/v1/auth/logout", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}