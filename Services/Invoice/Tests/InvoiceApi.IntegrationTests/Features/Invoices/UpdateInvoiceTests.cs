using System.Net;
using FluentAssertions;
using InvoiceApi.Contracts.Invoices;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InvoiceApi.IntegrationTests.Features.Invoices;

public class UpdateInvoiceTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = TestCase.CreateTestHttpClient(factory, "updateInvoiceEndpoint");
    
    [Fact]
    public async Task UpdateInvoice_withValidBody_ReturnsOkResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var createResult = await SharedMethods.CreateInvoice(_client);
        var updateModel = new UpdateInvoiceCommand(createResult.InvoiceId, "Paid");
        
        // act
        var response = await _client.PutAsync("api/v1/invoice", TestCase.CreateContext(updateModel));
        
        // asserts
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await TestCase.DeserializeResponse<InvoiceResponse>(response);
        result.InvoiceId.Should().Be(createResult.InvoiceId);
        result.Status.Should().Be(updateModel.Status);
    }

    [Fact]
    public async Task UpdateInvoice_withInvalidInvoice_ReturnsNotFoundResult()
    {
        // arrange
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "email@test.com"));
        var updateModel = new UpdateInvoiceCommand(Guid.NewGuid(), "Paid");
        
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
        var createResult = await SharedMethods.CreateInvoice(_client);
        var updateModel = new UpdateInvoiceCommand(createResult.InvoiceId, "Paid");
        TestCase.IncludeTokenInRequest(_client, TestCase.CreateFakeToken(Guid.NewGuid(), "user2@test.com"));

        // act
        var response = await _client.PutAsync("api/v1/invoice", TestCase.CreateContext(updateModel));
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}