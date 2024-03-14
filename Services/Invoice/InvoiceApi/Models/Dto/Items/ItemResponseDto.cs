using InvoiceApi.Models.Entities;

namespace InvoiceApi.Models.Dto.Items;

public sealed class ItemResponseDto()
{
    public Guid InvoiceId { get; init; }
    public string Name { get; init; }
    public int Amount { get; init; }
    public string Unit { get; init; }
    public string Vat { get; init; }
    public decimal NetPrice { get; init; }
    public decimal GrossPrice { get; init; }

    public ItemResponseDto(Item item) : this()
    {
        InvoiceId = item.InvoiceId;
        Name = item.Name;
        Amount = item.Amount;
        Unit = item.Unit.ToString();
        Vat = item.Vat.ToString();
        NetPrice = item.NetPrice;
        GrossPrice = item.GrossPrice;
    }
}