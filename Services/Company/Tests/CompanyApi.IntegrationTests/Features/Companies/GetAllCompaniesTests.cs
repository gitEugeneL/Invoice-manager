using System.Net;
using CompanyApi.Contracts;
using CompanyApi.Contracts.Companies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyApi.IntegrationTests.Features.Companies;

public class GetAllCompaniesTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "GetAllCompaniesEndpoint");
    
    [Theory]
    [InlineData(1, 10, 50)]
    [InlineData(2, 5, 20)]
    public async Task GetAllCompanies_withValidRequest_ReturnsOkResult(int pageNumber, int pageSize, int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        for (var i = 0; i < count; i++)
            await SharedMethods.CreateCompany(_client);
        
        var queryParams = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        
        // act
        var response = await _client.GetAsync($"api/v1/company{queryParams}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<PaginatedResponse<CompanyResponse>>(response);
        result.TotalItemsCount.Should().Be(count);
        result.Items.Count.Should().Be(pageSize);
        result.PageNumber.Should().Be(pageNumber);
    }

    [Fact]
    public async Task GetAllCompanies_withInvalidOwner_ReturnsEmptyResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.GetAsync("api/v1/company");
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<PaginatedResponse<CompanyResponse>>(response);
        result.TotalItemsCount.Should().Be(0);
    }
}