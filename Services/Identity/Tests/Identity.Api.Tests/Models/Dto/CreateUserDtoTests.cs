using FluentValidation.TestHelper;
using Identity.Api.Models.Dto;
using Xunit;

namespace Identity.Api.Tests.Models.Dto;

public class CreateUserDtoTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public void ValidCreateUserDto_PassesValidation()
    {
        // Arrange
        var model = new CreateUserDto("test@example.com", "strongPassword1@");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "strongPassword1@")] // Empty Email
    [InlineData("test@example.com", "")] // Empty Password
    [InlineData("test@example.com", "weak")] // Password too short
    [InlineData("notanemail", "strongPassword1@")] // Invalid Email format
    [InlineData("test@example.com", "strongPassword")] // Invalid password format
    public void InvalidCreateUserDto_FailsValidation(string email, string password)
    {
        // Arrange
        var model = new CreateUserDto(email, password);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}