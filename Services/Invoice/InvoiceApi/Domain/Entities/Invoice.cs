using InvoiceApi.Domain.Entities.Common;
using InvoiceApi.Domain.Entities.Enums;

namespace InvoiceApi.Domain.Entities;

public sealed class Invoice : BaseAuditableEntity
{
    public required Guid OwnerId { get; init; }
    public required Guid SellerCompanyId { get; init; }
    public required Guid BuyerCompanyId { get; init; }
    public required string Number { get; init; }
    public int TermsOfPayment { get; init; } 
    public required Payment PaymentType { get; init; }
    public required Status Status { get; set; }
    public bool Locked { get; set; }
    public decimal TotalNetPrice
    {
        get => Items.Sum(i => i.SumNetPrice);
        set { }
    }
    public decimal TotalGrossPrice
    {
        get => Items.Sum(i => i.GrossPrice);
        set { }
    }
    /** Relations **/
    public List<Item> Items { get; init; } = [];
}