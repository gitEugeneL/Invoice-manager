using InvoiceApi.Domain.Entities;

namespace InvoiceApi.Contracts.Invoices;

public sealed class InvoiceResponse()
{
    public Guid InvoiceId { get; init; }
    public Guid SellerCompanyId { get; init; }
    public Guid BuyerCompanyId { get; init; }
    public string Number { get; init; } = string.Empty;
    public decimal TotalNetPrice { get; init; } 
    public decimal TotalGrossPrice { get; init; } 
    public int TermsOfPayment { get; init; }
    public string PaymentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public List<Guid> ItemsId { get; init; } = []; 
    
    public InvoiceResponse(Invoice invoice) : this()
    {
        InvoiceId = invoice.Id;
        SellerCompanyId = invoice.SellerCompanyId;
        BuyerCompanyId = invoice.BuyerCompanyId;
        Number = invoice.Number;
        TotalNetPrice = invoice.TotalNetPrice;
        TotalGrossPrice = invoice.TotalGrossPrice;
        TermsOfPayment = invoice.TermsOfPayment;
        PaymentType = invoice.PaymentType.ToString();
        Status = invoice.Status.ToString();
        ItemsId = invoice.Items.Select(i => i.Id).ToList();
    }
}