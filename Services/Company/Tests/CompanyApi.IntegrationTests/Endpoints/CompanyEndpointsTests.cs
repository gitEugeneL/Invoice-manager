using System.Net;
using CompanyApi.Models.Dto;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyApi.IntegrationTests.Endpoints;

public class CompanyEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "companyEndpoints");


    private async Task<CompanyResponseDto> CreateCompany()
    {
        var model = new CreateCompanyDto(
            "Company", "5555555555", "Radom", "Warszawska", "123", "02-495");
        var response = await _client.PostAsync("api/v1/company", TestCase.CreateContext(model));
        return await TestCase.DeserializeResponse<CompanyResponseDto>(response);
    }
    
    [Theory]
    [InlineData("Company A", "1234567890", "New York", "Broadway", "123", "02-495")]
    [InlineData("Company B", "0987654321", "Los Angeles", "Hollywood Blvd", "456", "50-358")]
    [InlineData("Company C", "1357924680", "Chicago", "Michigan Ave", "789", "60601")]
    public async Task CreateCompany_withValidBody_ReturnsCreatedResult(
        string name,
        string taxNumber,
        string city,
        string street,
        string houseNumber,
        string postalCode)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var model = new CreateCompanyDto(name, taxNumber, city, street, houseNumber, postalCode);
        
        // act
        var response = await _client.PostAsync("api/v1/company", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateCompany_withValidBody_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateCompany();
        var updateModel = new UpdateCompanyDto(
            createResult.CompanyId, "updatedName", null, null, null,null, null);
        
        // act
        var response = await _client.PutAsync("api/v1/company", TestCase.CreateContext(updateModel));
    
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<CompanyResponseDto>(response);
        result.Name.Should().Be(updateModel.Name);
        result.CompanyId.Should().Be(createResult.CompanyId);
    }

    [Fact]
    public async Task UpdateCompany_withInvalidCompany_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var updateModel = new UpdateCompanyDto(Guid.NewGuid(), "updatedName", null, null, null,null, null);
        
        // act
        var response = await _client.PutAsync("api/v1/company", TestCase.CreateContext(updateModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCompany_withInvalidUser_ReturnsForbidResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await CreateCompany();
        var updateModel = new UpdateCompanyDto(
            createResult.CompanyId, "updatedName", null, null, null,null, null);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.PutAsync("api/v1/company", TestCase.CreateContext(updateModel));
        
        // assert 
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task GetCompanyById_withValidId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateCompany();
        
        // act
        var response = await _client.GetAsync($"api/v1/company/{createResult.CompanyId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<CompanyResponseDto>(response);
        result.CompanyId.Should().Be(createResult.CompanyId);
    }

    [Theory]
    [InlineData(1, 10, 50)]
    [InlineData(2, 5, 20)]
    public async Task GetAllCompanies_withValidRequest_ReturnsOkResult(int pageNumber, int pageSize, int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        for (var i = 0; i < count; i++)
            await CreateCompany();
        
        var queryParams = $"?pageNumber={pageNumber}&pageSize={pageSize}";
        
        // act
        var response = await _client.GetAsync($"api/v1/company{queryParams}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<PaginatedResponse<CompanyResponseDto>>(response);
        result.TotalItemsCount.Should().Be(count);
        result.Items.Count.Should().Be(pageSize);
        result.PageNumber.Should().Be(pageNumber);
    }

    [Fact]
    public async Task DeleteCompanyById_withValidId_ReturnsNoContent()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateCompany();
        
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
}
