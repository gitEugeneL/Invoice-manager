using CompanyApi.Models.Dto;
using FluentValidation.TestHelper;
using Xunit;

namespace CompanyApi.Tests.Models.Dto;

public class CreateCompanyDtoTests
{
    private readonly CreateCompanyValidator _validator = new();

    [Fact]
    public void ValidCreateUserDto_PassesValidation()
    {
        // Arrange
        var model = new CreateCompanyDto("Example Company", "1234567890", "City", "Street", "1", "12345");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData("", "1234567890", "City", "Street", "1", "12345")] // Empty Name
    [InlineData("Example Company", "", "City", "Street", "1", "12345")] // Empty TaxNumber
    [InlineData("Example Company", "123456789", "City", "Street", "1", "12345")] // TaxNumber too short
    [InlineData("Example Company", "12345678901", "City", "Street", "1", "12345")] // TaxNumber too long
    [InlineData("Example Company", "abc", "City", "Street", "1", "12345")] // Invalid TaxNumber format
    [InlineData("Example Company", "1234567890", "", "Street", "1", "12345")] // Empty City
    [InlineData("Example Company", "1234567890", "City", "", "1", "12345")] // Empty Street
    [InlineData("Example Company", "1234567890", "City", "Street", "", "12345")] // Empty HouseNumber
    [InlineData("Example Company", "1234567890", "City", "Street", "1", "")] // Empty PostalCode
    public void InvalidCreateCompanyDto_FailsValidation(string name, string taxNumber, string city, string street, string houseNumber, string postalCode)
    {
        // Arrange
        var model = new CreateCompanyDto(name, taxNumber, city, street, houseNumber, postalCode);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}