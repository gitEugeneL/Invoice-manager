using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts;
using InvoiceApi.Contracts.Invoices;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class GetAllInvoicesTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "getAllInvoicesEndpoint");

    [Theory]
    [InlineData(1, 10, 50)]
    [InlineData(2, 5, 20)]
    public async Task GetAllInvoices_withValidRequest_ReturnsOkResult(int pageNumber, int pageSize, int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        for (var i = 0; i < count; i++)
            await SharedMethods.CreateInvoice(_client);
        
        var queryParams = $"?pageNumber={pageNumber}&pageSize={pageSize}";

        // act
        var response = await _client.GetAsync($"api/v1/invoice{queryParams}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<PaginatedResponse<InvoiceResponse>>(response);
        result.TotalItemsCount.Should().Be(count);
        result.Items.Count.Should().Be(pageSize);
        result.PageNumber.Should().Be(pageNumber);
    }

}