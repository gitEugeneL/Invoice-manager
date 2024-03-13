using InvoiceApi.Data;
using InvoiceApi.Models.Entities;
using InvoiceApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Repositories;

internal class InvoiceRepository(AppDbContext context) : IInvoiceRepository
{
    public async Task<Invoice> CreateInvoice(Invoice invoice)
    {
        await context
            .Invoices
            .AddAsync(invoice);
        await context
            .SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateInvoice(Invoice invoice)
    {
        context
            .Update(invoice);
        await context
            .SaveChangesAsync();
        return invoice;
    }

    public async Task DeleteInvoice(Invoice invoice)
    {
        context
            .Remove(invoice);
        await context
            .SaveChangesAsync();
    }

    public async Task<Invoice?> FindInvoiceById(Guid invoiceId)
    {
        return await context
            .Invoices
            .Include(i => i.Items)
            .SingleOrDefaultAsync(i => i.Id == invoiceId);
    }

    public async Task<(IEnumerable<Invoice> List, int Count)> FindInvoicesByOwnerId(
        Guid ownerId, 
        int pageNumber, 
        int pageSize)
    {
        var query = context
            .Invoices
            .Where(i => i.OwnerId == ownerId)
            .Include(i => i.Items)
            .AsQueryable();

        var count = await query
            .CountAsync();

        var invoices = await query
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();

        return (invoices, count);
    }

    public async Task<int> CountInvoicesByMonthAndOwnerId(DateTime date, Guid ownerId)
    {
        return await context
            .Invoices
            .Where(i => i.OwnerId == ownerId 
                        && i.Created.Year == date.Year
                        && i.Created.Month == date.Month )
            .CountAsync();
    }
}