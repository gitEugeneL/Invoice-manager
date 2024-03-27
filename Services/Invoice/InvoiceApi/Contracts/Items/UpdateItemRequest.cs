namespace InvoiceApi.Contracts.Items;

public sealed record UpdateItemRequest(
    Guid ItemId,
    string? Name,
    int? Amount,
    decimal? NetPrice
);