using FluentValidation.TestHelper;
using InvoiceApi.Features.Items;
using Xunit;

namespace InvoiceApi.Tests.Features.Items;

public class UpdateItemRequestTests
{
    private readonly UpdateItem.Validator _validator = new();
    
    [Fact]
    public void ValidUpdateItemCommand_PassesValidation()
    {
        // arrange
        var model = new UpdateItem.Command
        (
            CurrentUserId: Guid.NewGuid(),
            ItemId: Guid.NewGuid(),
            Name: "Updated Item",
            Amount: 15,
            NetPrice: 150.75m);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData(0, 150.75)] // Amount less than 1
    [InlineData(1000000, 150.75)] // Amount greater than 999999
    [InlineData(15, 0)] // NetPrice less than 1
    public void InvalidUpdateItemCommand_FailsValidation(int amount, decimal netPrice)
    {
        // arrange
        var model = new UpdateItem.Command
        (
            CurrentUserId: Guid.NewGuid(),
            ItemId: Guid.NewGuid(),
            Name: "Updated Item",
            Amount: amount,
            NetPrice: netPrice);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}