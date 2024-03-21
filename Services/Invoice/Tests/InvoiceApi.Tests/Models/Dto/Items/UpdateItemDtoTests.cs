using FluentValidation.TestHelper;
using InvoiceApi.Models.Dto.Items;
using Xunit;

namespace InvoiceApi.Tests.Models.Dto.Items;

public class UpdateItemDtoTests
{
    private readonly UpdateItemValidator _validator = new();
    
    [Fact]
    public void ValidUpdateItemDto_PassesValidation()
    {
        // arrange
        var model = new UpdateItemDto(
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
    public void InvalidUpdateItemDto_FailsValidation(int amount, decimal netPrice)
    {
        // arrange
        var model = new UpdateItemDto(
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