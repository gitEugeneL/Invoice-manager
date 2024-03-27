using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Invoices;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class GetInvoiceByIdTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "getInvoiceByIdEndpoint");
    
    [Fact]
    public async Task GetInvoiceById_withValidInvoiceId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        
        // act
        var response = await _client.GetAsync($"api/v1/invoice/{createResult.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<InvoiceResponse>(response);
        result.InvoiceId.Should().Be(createResult.InvoiceId);
    }
    
    [Fact]
    public async Task GetInvoiceById_withInvalidInvoiceId_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.GetAsync($"api/v1/invoice/{Guid.NewGuid()}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetInvoiceById_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.GetAsync($"api/v1/invoice/{createResult.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}