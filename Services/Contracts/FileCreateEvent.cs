namespace Contracts;

public sealed record FileCreateEvent
{
    public required Guid InvoiceId { get; init; }
    public required Guid OwnerId { get; init; }
    public required Guid SellerCompanyId { get; init; }
    public required Guid BuyerCompanyId { get; init; }
    public required string Number { get; init; }
    public required decimal TotalNetPrice { get; init; } 
    public required decimal TotalGrossPrice { get; init; } 
    public required int TermsOfPayment { get; init; }
    public required string PaymentType { get; init; }
    public required string Status { get; init; }
    public required IEnumerable<Item> Items { get; init; } = []; 
}

public sealed record Item
{
    public required Guid ItemId { get; init; }
    public required string Name { get; init; }
    public required int Amount { get; init; }
    public required string Unit { get; init; }
    public required string Vat { get; init; }
    public required decimal NetPrice { get; init; }
    public required decimal SumNetPrice { get; init; } 
    public required decimal SumGrossPrice { get; init; }
}

