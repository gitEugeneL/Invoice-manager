using CompanyApi.Models.Dto;
using FluentValidation.TestHelper;
using Xunit;

namespace CompanyApi.Tests.Models.Dto;

public class UpdateCompanyDtoTests
{
    private readonly UpdateCompanyValidator _validator = new();

    [Fact]
    public void ValidUpdateCompanyDto_PassesValidation()
    {
        // Arrange
        var model = new UpdateCompanyDto(
            CompanyId: Guid.NewGuid(),
            Name: "Updated Company",
            TaxNumber: "1234567890",
            City: "Updated City",
            Street: "Updated Street",
            HouseNumber: "1",
            PostalCode: "12345"
        );

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData("Updated Company", "123456789", "City", "Street", "1", "12345")] // TaxNumber too short
    [InlineData("Updated Company", "12345678901", "City", "Street", "1", "12345")] // TaxNumber too long
    [InlineData("Updated Company", "abc", "City", "Street", "1", "12345")] // Invalid TaxNumber format
    [InlineData("Updated Company", "1234567890", "City", "Street", "1", "1234567891011")] // PostalCode too long
    public void InvalidUpdateCompanyDto_FailsValidation(string name, string taxNumber, string city, string street, string houseNumber, string postalCode)
    {
        // Arrange
        var model = new UpdateCompanyDto(
            CompanyId: Guid.NewGuid(),
            Name: name,
            TaxNumber: taxNumber,
            City: city,
            Street: street,
            HouseNumber: houseNumber,
            PostalCode: postalCode
        );

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}