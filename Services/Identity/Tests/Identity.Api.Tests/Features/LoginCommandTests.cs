using FluentValidation.TestHelper;
using Identity.Api.Features.Auth;
using Xunit;

namespace Identity.Api.Tests.Features;

public class LoginCommandTests
{
    private readonly Login.Validator _validator = new();

    [Fact]
    public void ValidLoginCommand_PassesValidation()
    {
        // arrange 
        var model = new Login.Command("test@example.com", "strongPassword1@");

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Theory]
    [InlineData("", "password")] // Empty Email
    [InlineData("test@example.com", "")] // Empty Password
    [InlineData("notanemail", "password")] // Invalid Email format
    [InlineData("test@example.com", "short")] // Password too short
    public void InvalidLoginCommand_FailsValidation(string email, string password)
    {
        // Arrange
        var model = new Login.Command(email, password);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}