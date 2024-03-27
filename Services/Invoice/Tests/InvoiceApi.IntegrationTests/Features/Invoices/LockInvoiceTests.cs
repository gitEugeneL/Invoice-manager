using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class LockInvoiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "lockInvoiceEndpoint");

    [Fact]
    public async Task LockInvoice_withValidId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        
        // act
        var response = await _client.PatchAsync($"api/v1/invoice/lock/{createResult.InvoiceId}", null);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<Guid>(response);
        result.Should().Be(createResult.InvoiceId);
    }

    [Fact]
    public async Task LockInvoice_withInvalidId_ReturnsNotFound()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.PatchAsync($"api/v1/invoice/lock/{Guid.NewGuid()}", null);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LockInvoice_withInvalidOwner_ReturnsNotFound()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.PatchAsync($"api/v1/invoice/lock/{createResult.InvoiceId}", null);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}