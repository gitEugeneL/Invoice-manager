using System.Net;
using FluentAssertions;
using InvoiceApi.Models.Dto.Invoices;
using InvoiceApi.Models.Dto.Items;
using InvoiceApi.Models.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Endpoints;

public class ItemEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "itemEndpoints");

    private async Task<ItemResponseDto> CreateItem(InvoiceResponseDto invoice)
    {
        var model = new CreateItemDto(
            invoice.InvoiceId, "Wall painting", 30, "Hours", "Vat23", 1000);
        var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
        return await TestCase.DeserializeResponse<ItemResponseDto>(response);
    }
    
    [Theory]
    [InlineData("Rustic decor", 5, "Items", "Vat23", 100)]
    [InlineData("Cozy Canvas", 1, "Days", "Vat0",  589)]
    [InlineData("Wall painting", 5, "Hours", "Vat5", 235)]
    [InlineData("Concrete", 58, "Kg", "Vat7",  457)]
    [InlineData("Delivery", 22500, "Km", "Vat8",  568.55)]
    public async Task CreateItem_withValidBody_ReturnsCreatedResult(
        string name,
        int amount,
        string unit,
        string vat,
        decimal netPrice)
    {
        // arrange 
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var model = new CreateItemDto(invoice.InvoiceId, name, amount, unit, vat, netPrice);

        // act
        var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await TestCase.DeserializeResponse<ItemResponseDto>(response);
        var sumNetPrice = model.NetPrice * model.Amount;
        
        result.NetPrice.Should().Be(model.NetPrice);
        result.Amount.Should().Be(model.Amount);
        
        result.SumNetPrice.Should().Be(sumNetPrice);
        result.SumGrossPrice.Should().Be(sumNetPrice + sumNetPrice / 100 * (decimal)Enum.Parse<Vat>(model.Vat));
    }
    
    [Fact]
    public async Task CreateItem_withInvalidInvoice_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var model = new CreateItemDto(
            Guid.NewGuid(), "Product1", 1, "Items", "Vat23", 10);
        
        // act
        var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateItem_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var model = new CreateItemDto(
            invoice.InvoiceId, "Product1", 1, "Items", "Vat23", 10);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));

        // act
        var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task CreateItem_withLockedInvoice_ReturnsNotFoundResult()
    {
       // arrange
       TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
       var invoice = await TestCase.CreateInvoice(_client);
       var model = new CreateItemDto(
           invoice.InvoiceId, "Product1", 1, "Items", "Vat23", 10);
       await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
       
       // act
       var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
       
       // assert
       response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateItem_withValidBody_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
        var model = new UpdateItemDto(item.ItemId, "newName", 2, 99);
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<ItemResponseDto>(response);
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
        var model = new UpdateItemDto(Guid.NewGuid(), "newName", 2, 99);
        
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
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
        var model = new UpdateItemDto(item.ItemId, "newName", 2, 99);
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
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
        var model = new UpdateItemDto(item.ItemId, "newName", 2, 99);
        await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
        
        // act
        var response = await _client.PutAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetItemById_withValidId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);

        // act
        var response = await _client.GetAsync($"api/v1/item/{item.ItemId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetItemById_withInvalidId_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        
        // act
        var response = await _client.GetAsync($"api/v1/item/{Guid.NewGuid()}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetItemById_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));

        // act
        var response = await _client.GetAsync($"api/v1/item/{item.ItemId}");

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(5)]
    [InlineData(1)]
    [InlineData(0)]
    public async Task GetItemsByInvoiceId_withValidInvoiceId_ReturnsOkResult(int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);

        for (var i = 0; i < count; i++)
            await CreateItem(invoice);
        
        // act
        var response = await _client.GetAsync($"api/v1/item/all-by-invoice/{invoice.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<List<ItemResponseDto>>(response);
        result.Count.Should().Be(count);
        result.All(item => item.InvoiceId == invoice.InvoiceId).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteItem_withValidItemId_ReturnsNoContentResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);

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
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
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
        var invoice = await TestCase.CreateInvoice(_client);
        var item = await CreateItem(invoice);
        await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
        
        // act
        var response = await _client.DeleteAsync($"api/v1/item/{item.ItemId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}