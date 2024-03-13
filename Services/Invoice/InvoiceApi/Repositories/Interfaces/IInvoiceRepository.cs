using InvoiceApi.Models.Entities;

namespace InvoiceApi.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice> CreateInvoice(Invoice invoice);
    Task<Invoice> UpdateInvoice(Invoice invoice);
    Task DeleteInvoice(Invoice invoice);
    Task<Invoice?> FindInvoiceById(Guid invoiceId);
    Task<(IEnumerable<Invoice> List, int Count)> FindInvoicesByOwnerId(Guid ownerId, int pageNumber, int pageSize);
    Task<int> CountInvoicesByMonthAndOwnerId(DateTime date, Guid ownerId);
}