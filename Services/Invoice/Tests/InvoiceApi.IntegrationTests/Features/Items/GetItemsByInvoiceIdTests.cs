using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Items;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Items;

public class GetItemsByInvoiceIdTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "getItemsByInvoiceIdEndpoint");
    
    [Theory]
    [InlineData(10)]
    [InlineData(5)]
    [InlineData(1)]
    [InlineData(0)]
    public async Task GetItemsByInvoiceId_withValidInvoiceId_ReturnsOkResult(int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var invoice = await SharedMethods.CreateInvoice(_client);

        for (var i = 0; i < count; i++)
            await SharedMethods.CreateItem(invoice, _client);
        
        // act
        var response = await _client.GetAsync($"api/v1/item/all-by-invoice/{invoice.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<List<ItemResponse>>(response);
        result.Count.Should().Be(count);
        result.All(item => item.InvoiceId == invoice.InvoiceId).Should().BeTrue();
    }
}