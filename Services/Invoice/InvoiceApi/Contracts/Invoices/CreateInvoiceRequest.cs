namespace InvoiceApi.Contracts.Invoices;

public sealed record CreateInvoiceRequest(
    Guid SellerCompanyId,
    Guid BuyerCompanyId,
    int TermsOfPayment,
    string PaymentType,
    string Status
);