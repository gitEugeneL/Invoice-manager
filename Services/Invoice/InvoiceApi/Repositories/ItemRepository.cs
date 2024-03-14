using InvoiceApi.Data;
using InvoiceApi.Models.Entities;
using InvoiceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Repositories;

internal class ItemRepository(AppDbContext context) : IItemRepository
{
    public async Task<Item> CreateItem(Item item)
    {
        await context
            .Items
            .AddAsync(item);
        await context
            .SaveChangesAsync();
        return item;
    }

    public async Task<Item> UpdateItem(Item item)
    {
        context
            .Items
            .Update(item);
        await context
            .SaveChangesAsync();
        return item;
    }

    public async Task DeleteItem(Item item)
    {
        context
            .Items
            .Remove(item);
        await context
            .SaveChangesAsync();
    }

    public async Task<Item?> FindItemById(Guid itemId)
    {
        return await context
            .Items
            .Include(i => i.Invoice)
            .SingleOrDefaultAsync(i => i.Id == itemId);
    }

    public async Task<IEnumerable<Item>> FindAllByInvoiceIdForOwner(Guid invoiceId, Guid ownerId)
    {
        return await context
            .Items
            .Include(i => i.Invoice)
            .Where(i => i.InvoiceId == invoiceId
                        && i.Invoice.OwnerId == ownerId)
            .ToListAsync();
    }
}