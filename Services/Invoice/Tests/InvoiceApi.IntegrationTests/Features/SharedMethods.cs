using System.Net.Http.Json;
using InvoiceApi.Contracts.Invoices;
using InvoiceApi.Contracts.Items;

namespace InvoiceApi.IntegrationTests.Features;

public static class SharedMethods
{
    public static async Task<InvoiceResponse> CreateInvoice(HttpClient client)
    {
        var model = new CreateInvoiceRequest(
            Guid.NewGuid(), Guid.NewGuid(), 3, "Cash", "Unpaid");
        var response = await client.PostAsJsonAsync("api/v1/invoice", model);
        return await TestCase.DeserializeResponse<InvoiceResponse>(response);
    }
    
    public static async Task<ItemResponse> CreateItem(InvoiceResponse invoice, HttpClient client)
    {
        var model = new CreateItemCommand(
            invoice.InvoiceId, "Wall painting", 30, "Hours", "Vat23", 1000);
        var response = await client.PostAsJsonAsync("api/v1/item", model);
        return await TestCase.DeserializeResponse<ItemResponse>(response);
    }
}