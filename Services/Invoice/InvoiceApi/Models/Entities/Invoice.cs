using InvoiceApi.Models.Entities.Common;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Entities;

public sealed class Invoice : BaseAuditableEntity
{
    public required Guid OwnerId { get; init; }
    public required Guid SellerCompanyId { get; init; }
    public required Guid BuyerCompanyId { get; init; }
    public required string Number { get; init; }
    public decimal TotalNetPrice { get; init; } 
    public decimal TotalGrossPrice { get; init; } 
    public int TermsOfPayment { get; init; } 
    public required Payment PaymentType { get; init; }
    public required Status Status { get; set; }

    /** Relations **/
    public List<Item> Items { get; init; } = [];
    
    public Invoice()
    {
        TotalGrossPrice = Items.Sum(i => i.GrossPrice);
        TotalNetPrice = Items.Sum(i => i.NetPrice);
    }
}