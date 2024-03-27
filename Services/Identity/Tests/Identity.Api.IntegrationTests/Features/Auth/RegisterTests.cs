using System.Net;
using FluentAssertions;
using IdentityApi.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Identity.Api.IntegrationTests.Features.Auth;

public class RegisterTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "registerEndpoint");
    
    [Theory]
    [InlineData("mailt@mail.test", "strongPwd!1")]
    [InlineData("mail1@mail.test", "myPassword12@")]
    public async Task Register_withValidBody_ReturnsCreatedResult(string email, string password)
    {
         // arrange
         var model = new RegisterRequest(email, password);

         // act
         var response = await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(model));
         
         // assert
         response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_withExistingUser_ReturnsConflictResult()
    {
         // arrange
         var model = new RegisterRequest("test@email.com", "strongPwd!1");
         
         // act
         var response  = new HttpResponseMessage();
         for (var i = 0; i < 2; i++)
             response = await _client.PostAsync("api/v1/auth/register", TestCase.CreateContext(model));
         
         // assert
         response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}