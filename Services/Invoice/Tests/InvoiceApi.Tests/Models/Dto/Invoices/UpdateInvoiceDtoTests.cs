using FluentValidation.TestHelper;
using InvoiceApi.Models.Dto.Invoices;
using Xunit;

namespace InvoiceApi.Tests.Models.Dto.Invoices;

public class UpdateInvoiceDtoTests
{
    private readonly UpdateInvoiceValidator _validator = new();
    
    [Fact]
    public void ValidUpdateInvoiceDto_PassesValidation()
    {
        // arrange
        var model = new UpdateInvoiceDto(
            InvoiceId: Guid.NewGuid(),
            Status: "Paid");

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidUpdateInvoiceDto_FailsValidation()
    {
        // arrange
        var model = new UpdateInvoiceDto(
            InvoiceId: Guid.Empty,
            Status: "InvalidStatus");

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}