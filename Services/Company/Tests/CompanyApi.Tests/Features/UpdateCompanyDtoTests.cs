using CompanyApi.Features.Companies;
using FluentValidation.TestHelper;
using Xunit;

namespace CompanyApi.Tests.Features;

public class UpdateCompanyDtoTests
{
    private readonly UpdateCompany.Validator _validator = new();

    [Fact]
    public void ValidUpdateCompanyDto_PassesValidation()
    {
        // arrange
        var model = new UpdateCompany.Command
        {
            CurrentUserId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Name = "Updated Company",
            TaxNumber = "1234567890",
            City = "Updated City",
            Street = "Updated Street",
            HouseNumber = "1",
            PostalCode = "12345"
        };
        
        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData("Updated Company", "123456789", "City", "Street", "1", "12345")] // TaxNumber too short
    [InlineData("Updated Company", "12345678901", "City", "Street", "1", "12345")] // TaxNumber too long
    [InlineData("Updated Company", "abc", "City", "Street", "1", "12345")] // Invalid TaxNumber format
    [InlineData("Updated Company", "1234567890", "City", "Street", "1", "1234567891011")] // PostalCode too long
    public void InvalidUpdateCompanyDto_FailsValidation(string name, string taxNumber, string city, string street, string houseNumber, string postalCode)
    {
        // arrange
        var model = new UpdateCompany.Command
        {
            CurrentUserId = Guid.NewGuid(),
            CompanyId = Guid.NewGuid(),
            Name = name,
            TaxNumber = taxNumber,
            City = city,
            Street = street,
            HouseNumber = houseNumber,
            PostalCode = postalCode
        };

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}