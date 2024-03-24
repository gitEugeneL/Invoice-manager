using CompanyApi.Features.Companies;
using FluentValidation.TestHelper;
using Xunit;

namespace CompanyApi.Tests.Features;

public class CreateCompanyCommandTests
{
    private readonly CreateCompany.Validator _validator = new();

    [Fact]
    public void ValidCreateUserDto_PassesValidation()
    {
        // arrange
        var model = new CreateCompany.Command()
        {
            CurrentUserId = Guid.NewGuid(),
            Name = "Example Company",
            TaxNumber = "1234567890",
            City = "City",
            Street = "Street",
            HouseNumber = "1A",
            PostalCode = "12-456"
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
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
        // arrange
        var model = new CreateCompany.Command()
        {
            CurrentUserId = Guid.NewGuid(),
            Name = name,
            TaxNumber = taxNumber,
            City = city,
            Street = street,
            HouseNumber = houseNumber,
            PostalCode = postalCode
        };

        // act
        var result = _validator.TestValidate(model);

        // asssert
        result.ShouldHaveAnyValidationError();
    }
}