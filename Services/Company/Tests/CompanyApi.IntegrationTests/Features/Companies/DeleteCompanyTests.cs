using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyApi.IntegrationTests.Features.Companies;

public class DeleteCompanyTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "DeleteEndpoint");
    
    [Fact]
    public async Task DeleteCompanyById_withValidId_ReturnsNoContent()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateCompany(_client);
        
        // act
        var response = await _client.DeleteAsync($"api/v1/company/{createResult.CompanyId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCompanyById_withInvalidId_ReturnsNotFound()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/company/{Guid.NewGuid()}");
    
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteCompanyById_withInvalidOwner_ReturnsNotFound()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await SharedMethods.CreateCompany(_client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/company/{createResult.CompanyId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}