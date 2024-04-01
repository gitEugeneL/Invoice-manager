using InvoiceApi.Domain.Entities.Common;
using InvoiceApi.Domain.Entities.Enums;

namespace InvoiceApi.Domain.Entities;

public sealed class Item : BaseEntity
{
    public required string Name { get; set; }
    public required int Amount { get; set; }
    public required Unit Unit { get; init; }
    public required Vat Vat { get; init; }
    public required decimal NetPrice { get; set; }

    public decimal SumNetPrice
    {
        get => NetPrice * Amount;
        set { }
    }
    
    public decimal GrossPrice
    {
        get => SumNetPrice + SumNetPrice / 100 * (decimal)Vat;
        set { }
    }

    /** Relations **/
    public Guid InvoiceId { get; init; }
    public required Invoice Invoice { get; init; }
}
