using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Items;
using InvoiceApi.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Items;

public class CreateItemTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "createItemEndpoint");
    
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
        var invoice = await SharedMethods.CreateInvoice(_client);
        var model = new CreateItemCommand(invoice.InvoiceId, name, amount, unit, vat, netPrice);

        // act
        var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await TestCase.DeserializeResponse<ItemResponse>(response);
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
        var model = new CreateItemCommand(
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
        var invoice = await SharedMethods.CreateInvoice(_client);
        var model = new CreateItemCommand(
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
       var invoice = await SharedMethods.CreateInvoice(_client);
       var model = new CreateItemCommand(
           invoice.InvoiceId, "Product1", 1, "Items", "Vat23", 10);
       var r = await _client.PatchAsync($"api/v1/invoice/lock/{invoice.InvoiceId}", null);
       
       // act
       var response = await _client.PostAsync("api/v1/item", TestCase.CreateContext(model));
       
       // assert
       response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}