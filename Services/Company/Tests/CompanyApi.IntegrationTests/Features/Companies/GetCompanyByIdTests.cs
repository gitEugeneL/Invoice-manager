using System.Net;
using CompanyApi.Contracts.Companies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyApi.IntegrationTests.Features.Companies;

public class GetCompanyByIdTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "GetCompanyByIdEndpoint");
    
    [Fact]
    public async Task GetCompanyById_withValidId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateCompany(_client);
        
        // act
        var response = await _client.GetAsync($"api/v1/company/{createResult.CompanyId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<CompanyResponse>(response);
        result.CompanyId.Should().Be(createResult.CompanyId);
    }

    [Fact]
    public async Task GetCompanyById_withInvalidId_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.GetAsync($"api/v1/company/{Guid.NewGuid()}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetCompanyById_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await SharedMethods.CreateCompany(_client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.GetAsync($"api/v1/company/{createResult.CompanyId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}