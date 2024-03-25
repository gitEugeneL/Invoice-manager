using FluentValidation.TestHelper;
using Identity.Api.Features.Auth;
using Xunit;

namespace Identity.Api.Tests.Features;

public class RegisterCommandTests
{
    private readonly Register.Validator _validator = new();

    [Fact]
    public void ValidRegisterCommand_PassesValidation()
    {
        // Arrange
        var model = new Register.Command("test@example.com", "strongPassword1@");

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
    public void InvalidRegisterCommand_FailsValidation(string email, string password)
    {
        // Arrange
        var model = new Register.Command(email, password);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}