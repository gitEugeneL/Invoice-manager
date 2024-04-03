using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Invoices;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class CreateInvoiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "createInvoiceEndpoint");
    
    [Theory]
    [InlineData(7, "Blik", "Unpaid")]
    [InlineData(14, "Przelewy24", "Unpaid")]
    [InlineData(3, "PayU", "Unpaid")]
    [InlineData(30, "DebitCard", "Unpaid")]
    [InlineData(0, "Cash", "Paid")]
    public async Task CreateInvoice_withValidBody_ReturnsCreatedResult(
        int termsOfPayment, 
        string paymentType, 
        string status)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var model = new CreateInvoiceRequest(
            Guid.NewGuid(), Guid.NewGuid(), termsOfPayment, paymentType, status);
        
        // act
        var response = await _client.PostAsync("api/v1/invoice", TestCase.CreateContext(model));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await TestCase.DeserializeResponse<InvoiceResponse>(response);
        var date = DateTime.Now;
        result.Number.Should().Be($"FV-1/{date.Month:MM}/{date.Year}");
       }
}