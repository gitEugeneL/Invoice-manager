using FluentValidation.TestHelper;
using InvoiceApi.Models.Dto.Items;
using Xunit;

namespace InvoiceApi.Tests.Models.Dto.Items;

public class CreateItemDtoTests
{
    private readonly CreateItemValidator _validator = new();
    
    [Fact]
    public void ValidCreateItemDto_PassesValidation()
    {
        // arrange
        var model = new CreateItemDto(
            InvoiceId: Guid.NewGuid(),
            Name: "Sample Item",
            Amount: 10,
            Unit: "Items",
            Vat: "Vat23",
            NetPrice: 100.50m);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData("", 10, "Items", "Vat23", 100.50)] // Empty Name
    [InlineData("Sample Item", 0, "Items", "Vat23", 100.50)] // Amount less than 1
    [InlineData("Sample Item", 1000000, "Items", "Vat23", 100.50)] // Amount greater than 999999
    [InlineData("Sample Item", 10, "", "Vat23", 100.50)] // Empty Unit
    [InlineData("Sample Item", 10, "InvalidUnit", "Vat23", 100.50)] // Invalid Unit
    [InlineData("Sample Item", 10, "Items", "", 100.50)] // Empty Vat
    [InlineData("Sample Item", 10, "Items", "InvalidVat", 100.50)] // Invalid Vat
    [InlineData("Sample Item", 10, "Items", "Vat23", 0)] // NetPrice less than 1
    public void InvalidCreateItemDto_FailsValidation(string name, int amount, string unit, string vat, decimal netPrice)
    {
        // Arrange
        var model = new CreateItemDto(
            InvoiceId: Guid.NewGuid(),
            Name: name,
            Amount: amount,
            Unit: unit,
            Vat: vat,
            NetPrice: netPrice);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}