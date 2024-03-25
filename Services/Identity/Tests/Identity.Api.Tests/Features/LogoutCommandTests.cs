using System.Security.Cryptography;
using FluentValidation.TestHelper;
using Identity.Api.Features.Auth;
using Xunit;

namespace Identity.Api.Tests.Features;

public class LogoutCommandTests
{
    private readonly Logout.Validator _validator = new();

    [Fact]
    public void ValidLogoutCommand_PassesValidation()
    {
        // arrange
        var model = new Logout.Command(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidLogoutCommand_FailsValidation()
    {
        // arrange
        var model = new Logout.Command(null!);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}