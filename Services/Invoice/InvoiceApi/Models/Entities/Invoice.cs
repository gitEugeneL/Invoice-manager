using InvoiceApi.Models.Entities.Common;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Entities;

public sealed class Invoice : BaseAuditableEntity
{
    public required Guid SellerId { get; init; }
    public required Guid BuyerId { get; init; }
    public required string Number { get; init; }
    public required decimal TotalGrossPrice { get; init; } 
    public required decimal TotalNetPrice { get; init; }
    public int TermsOfPayment { get; init; } 
    public required Payment PaymentType { get; init; }
    public required Status Status { get; init; }

    /** Relations **/
    public List<Item> Items { get; init; } = [];
}