using InvoiceApi.Domain.Entities;

namespace InvoiceApi.Contracts.Items;

public sealed class ItemResponse()
{
    public Guid InvoiceId { get; init; }
    public Guid ItemId { get; init; }
    public string Name { get; init; }
    public int Amount { get; init; }
    public string Unit { get; init; }
    public string Vat { get; init; }
    public decimal NetPrice { get; init; }
    public decimal SumNetPrice { get; init; } 
    public decimal SumGrossPrice { get; init; }

    public ItemResponse(Item item) : this()
    {
        InvoiceId = item.InvoiceId;
        ItemId = item.Id;
        Name = item.Name;
        Amount = item.Amount;
        Unit = item.Unit.ToString();
        Vat = item.Vat.ToString();
        NetPrice = item.NetPrice;
        SumNetPrice = item.SumNetPrice;
        SumGrossPrice = item.GrossPrice;
    }
}