using FluentValidation.TestHelper;
using InvoiceApi.Features.Invoices;
using Xunit;

namespace InvoiceApi.Tests.Features.Invoices;

public class UpdateInvoiceCommandTests
{
    private readonly UpdateInvoice.Validator _validator = new();
    
    [Fact]
    public void ValidUpdateInvoiceCommand_PassesValidation()
    {
        // arrange
        var model = new UpdateInvoice.Command
        (
            CurrentUserId: Guid.NewGuid(),
            InvoiceId: Guid.NewGuid(),
            Status: "Paid"
        );

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidUpdateInvoiceCommand_FailsValidation()
    {
        // arrange
        var model = new UpdateInvoice.Command
        (
            CurrentUserId: Guid.Empty,
            InvoiceId: Guid.Empty,
            Status: "InvalidStatus");

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}