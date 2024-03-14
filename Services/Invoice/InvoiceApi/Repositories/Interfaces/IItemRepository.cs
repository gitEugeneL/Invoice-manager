using InvoiceApi.Models.Entities;

namespace InvoiceApi.Repositories.Interfaces;

public interface IItemRepository
{
    Task<Item> CreateItem(Item item);
    Task<Item> UpdateItem(Item item);
    Task DeleteItem(Item item);
    Task<Item?> FindItemById(Guid itemId);
    Task<IEnumerable<Item>> FindAllByInvoiceIdForOwner(Guid invoiceId, Guid ownerId);
}