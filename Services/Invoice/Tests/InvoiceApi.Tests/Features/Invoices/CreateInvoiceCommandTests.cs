using FluentValidation.TestHelper;
using InvoiceApi.Features.Invoices;
using Xunit;

namespace InvoiceApi.Tests.Features.Invoices;

public class CreateInvoiceCommandTests
{
    private readonly CreateInvoice.Validator _validator = new();
    
    [Fact]
    public void ValidCreateInvoiceCommand_PassesValidation()
    {
        // arrange
        var model = new CreateInvoice.Command
        (
            CurrentUserId: Guid.NewGuid(),
            SellerCompanyId: Guid.NewGuid(),
            BuyerCompanyId: Guid.NewGuid(),
            TermsOfPayment: 30,
            PaymentType: "Transfer",
            Status: "Unpaid"
        );

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
    public void InvalidCreateInvoiceCommand_FailsValidation(int termsOfPayment, string paymentType, string status)
    {
        // arrange
        var model = new CreateInvoice.Command
        (
            CurrentUserId: Guid.NewGuid(),
            SellerCompanyId: Guid.NewGuid(),
            BuyerCompanyId: Guid.NewGuid(),
            TermsOfPayment: termsOfPayment,
            PaymentType: paymentType,
            Status: status
        );

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}