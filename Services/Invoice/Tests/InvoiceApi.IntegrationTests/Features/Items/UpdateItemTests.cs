using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Items;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Items;

public class UpdateItemTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "updateItemEndpoint");
    
    [Fact]
    public async Task UpdateItem_withValidBody_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);
        var model = new UpdateItemRequest(item.ItemId, "newName", 2, 99);
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<ItemResponse>(response);
        result.Amount.Should().Be(model.Amount);
        result.InvoiceId.Should().Be(invoice.InvoiceId);
        result.ItemId.Should().Be(item.ItemId);
        result.NetPrice.Should().Be(model.NetPrice);
        result.Name.Should().Be(model.Name);
    }
    
    [Fact]
    public async Task UpdateItem_withInvalidItem_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var model = new UpdateItemRequest(Guid.NewGuid(), "newName", 2, 99);
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateItem_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);
        var model = new UpdateItemRequest(item.ItemId, "newName", 2, 99);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateItem_withLockedInvoice_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);
        var item = await SharedMethods.CreateItem(invoice, _client);
        var model = new UpdateItemRequest(item.ItemId, "newName", 2, 99);
        await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}