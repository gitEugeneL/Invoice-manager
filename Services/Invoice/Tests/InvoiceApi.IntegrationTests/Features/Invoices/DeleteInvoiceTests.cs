using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class DeleteInvoiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "deleteInvoiceEndpoint");
    
    
    [Fact]
    public async Task DeleteInvoice_withValidId_ReturnsNoContentResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);

        // act
        var response = await _client.DeleteAsync($"api/v1/invoice/{createResult.InvoiceId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvoice_withInvalidId_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/invoice/{Guid.NewGuid()}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteInvoice_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/invoice/{createResult.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}