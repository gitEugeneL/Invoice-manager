using FluentValidation.TestHelper;
using Identity.Api.Models.Dto;
using Xunit;

namespace Identity.Api.Tests.Models.Dto;

public class LoginDtoTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void ValidLoginDto_PassesValidation()
    {
        // arrange 
        var model = new LoginDto("test@example.com", "strongPassword1@");

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
    public void InvalidLoginDto_FailsValidation(string email, string password)
    {
        // Arrange
        var loginDto = new LoginDto(email, password);

        // Act
        var result = _validator.TestValidate(loginDto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }
}
