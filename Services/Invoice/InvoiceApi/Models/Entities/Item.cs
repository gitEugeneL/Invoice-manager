using InvoiceApi.Models.Entities.Common;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Entities;

public sealed class Item(string name, int amount, Unit unit, Vat vat, decimal netPrice)
    : BaseEntity
{
    public required string Name { get; init; } = name;
    public required int Amount { get; init; } = amount;
    public required Unit Unit { get; init; } = unit;
    public required Vat Vat { get; init; } = vat;
    public required decimal NetPrice { get; init; } = netPrice;
    public decimal GrossPrice { get; init; } = netPrice + netPrice / 100 * (decimal)vat;

    /** Relations **/
    public Guid InvoiceId { get; init; }
    public required Invoice Invoice { get; init; }
}