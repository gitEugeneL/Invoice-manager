using InvoiceApi.Models.Entities;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Dto;

public sealed class InvoiceResponseDto()
{
    public Guid SellerCompanyId { get; init; }
    public Guid BuyerCompanyId { get; init; }
    public string Number { get; init; } = string.Empty;
    public decimal TotalNetPrice { get; init; } 
    public decimal TotalGrossPrice { get; init; } 
    public int TermsOfPayment { get; init; }
    public string PaymentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    
    // todo add Items list

    public InvoiceResponseDto(Invoice invoice) : this()
    {
        SellerCompanyId = invoice.SellerCompanyId;
        BuyerCompanyId = invoice.BuyerCompanyId;
        Number = invoice.Number;
        TotalNetPrice = invoice.TotalNetPrice;
        TotalGrossPrice = invoice.TotalGrossPrice;
        TermsOfPayment = invoice.TermsOfPayment;
        PaymentType = invoice.PaymentType.ToString();
        Status = invoice.Status.ToString();
    }
}