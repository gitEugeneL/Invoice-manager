using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Items;

public class DeleteInvoiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "deleteItemEndpoint");
    
    [Fact]
    public async Task DeleteItem_withValidItemId_ReturnsNoContentResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);

        // act
        var response = await _client.DeleteAsync($"api/v1/item/{item.ItemId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteItem_withInvalidItemId_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));

        // act
        var response = await _client.DeleteAsync($"api/v1/item/{Guid.NewGuid()}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteItem_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/item/{item.ItemId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteItem_withLockedInvoice_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);
        await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
        
        // act
        var response = await _client.DeleteAsync($"api/v1/item/{item.ItemId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}