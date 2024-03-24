using System.Net;
using CompanyApi.Contracts.Companies;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompanyApi.IntegrationTests.Features.Companies;

public class CreateCompanyTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "CreateEndpoint");
    
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
        var model = new CreateCompanyRequest(name, taxNumber, city, street, houseNumber, postalCode);
        
        // act
        var response = await _client.PostAsync("api/v1/company", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}