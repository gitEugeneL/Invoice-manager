using FluentValidation.TestHelper;
using InvoiceApi.Models.Dto.Invoices;
using Xunit;

namespace InvoiceApi.Tests.Models.Dto.Invoices;

public class CreateInvoiceDtoTests
{
    private readonly CreateInvoiceValidator _validator = new();
    
    [Fact]
    public void ValidCreateInvoiceDto_PassesValidation()
    {
        // arrange
        var model = new CreateInvoiceDto(
            SellerCompanyId: Guid.NewGuid(),
            BuyerCompanyId: Guid.NewGuid(),
            TermsOfPayment: 30,
            PaymentType: "Transfer",
            Status: "Unpaid");

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData(-1, "Transfer", "Unpaid")] // Negative TermsOfPayment
    [InlineData(101,"Transfer", "Unpaid")] // TermsOfPayment greater than 100
    [InlineData(30, "", "Unpaid")] // Empty PaymentType
    [InlineData(30, "InvalidPaymentType", "Unpaid")] // Invalid PaymentType
    [InlineData(30, "Transfer", "")] // Empty Status
    [InlineData(30, "Transfer", "InvalidStatus")] // Invalid Status
    public void InvalidCreateInvoiceDto_FailsValidation(int termsOfPayment, string paymentType, string status)
    {
        // arrange
        var model = new CreateInvoiceDto(
            SellerCompanyId: Guid.NewGuid(),
            BuyerCompanyId: Guid.NewGuid(),
            TermsOfPayment: termsOfPayment,
            PaymentType: paymentType,
            Status: status);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}