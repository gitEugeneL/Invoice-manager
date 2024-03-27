namespace InvoiceApi.Contracts.Items;

public sealed record CreateItemCommand(
    Guid InvoiceId,
    string Name,
    int Amount,
    string Unit,
    string Vat,
    decimal NetPrice
);