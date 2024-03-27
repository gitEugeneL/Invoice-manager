namespace InvoiceApi.Contracts.Invoices;

public sealed record UpdateInvoiceCommand(
    Guid InvoiceId,
    string Status
);