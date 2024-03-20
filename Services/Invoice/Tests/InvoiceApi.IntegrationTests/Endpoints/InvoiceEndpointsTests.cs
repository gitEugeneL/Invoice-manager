using System.Net;
using FluentAssertions;
using InvoiceApi.Models.Dto;
using InvoiceApi.Models.Dto.Invoices;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Endpoints;

public class InvoiceEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "invoiceEndpoints");

    private async Task<InvoiceResponseDto> CreateInvoice()
    {
        var model = new CreateInvoiceDto(
            Guid.NewGuid(), Guid.NewGuid(), 3, "Cash", "Unpaid");
        var response = await _client.PostAsync("api/v1/invoice", TestCase.CreateContext(model));
        return await TestCase.DeserializeResponse<InvoiceResponseDto>(response);
    }
    
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
        var model = new CreateInvoiceDto(
            Guid.NewGuid(), Guid.NewGuid(), termsOfPayment, paymentType, status);

        // act
        var response = await _client.PostAsync("api/v1/invoice", TestCase.CreateContext(model));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await TestCase.DeserializeResponse<InvoiceResponseDto>(response);
        var date = DateTime.Now;
        result.Number.Should().Be($"FV-1/{date.Month}/{date.Year}");
    }

    [Fact]
    public async Task UpdateInvoice_withValidBody_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateInvoice();
        var updateModel = new UpdateInvoiceDto(createResult.InvoiceId, "Paid");
        
        // act
        var response = await _client.PutAsync("api/v1/invoice", TestCase.CreateContext(updateModel));
        
        // asserts
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<InvoiceResponseDto>(response);
        result.InvoiceId.Should().Be(createResult.InvoiceId);
        result.Status.Should().Be(updateModel.Status);
    }

    [Fact]
    public async Task UpdateInvoice_withInvalidInvoice_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var updateModel = new UpdateInvoiceDto(Guid.NewGuid(), "Paid");
        
        // act
        var response = await _client.PutAsync("api/v1/invoice", TestCase.CreateContext(updateModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateInvoice_withInvalidOwner_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user1@test.com"));
        var createResult = await CreateInvoice();
        var updateModel = new UpdateInvoiceDto(createResult.InvoiceId, "Paid");
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));

        // act
        var response = await _client.PutAsync("api/v1/invoice", TestCase.CreateContext(updateModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Theory]
    [InlineData(1, 10, 50)]
    [InlineData(2, 5, 20)]
    public async Task GetAllInvoices_withValidRequest_ReturnsOkResult(int pageNumber, int pageSize, int count)
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        for (var i = 0; i < count; i++)
            await CreateInvoice();
        
        var queryParams = $"?pageNumber={pageNumber}&pageSize={pageSize}";

        // act
        var response = await _client.GetAsync($"api/v1/invoice{queryParams}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<PaginatedResponse<InvoiceResponseDto>>(response);
        result.TotalItemsCount.Should().Be(count);
        result.Items.Count.Should().Be(pageSize);
        result.PageNumber.Should().Be(pageNumber);
    }

    [Fact]
    public async Task DeleteInvoice_withValidId_ReturnsNoContentResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateInvoice();

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
        var createResult = await CreateInvoice();
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.DeleteAsync($"api/v1/invoice/{createResult.InvoiceId}");
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task LockInvoice_withValidId_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await CreateInvoice();
        
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
        var createResult = await CreateInvoice();
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));
        
        // act
        var response = await _client.PatchAsync($"api/v1/invoice/lock/{createResult.InvoiceId}", null);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}