using System.Security.Cryptography;
using FluentValidation.TestHelper;
using Identity.Api.Features.Auth;
using Xunit;

namespace Identity.Api.Tests.Features;

public class RefreshCommandTests
{
    private readonly Refresh.Validator _validator = new();

    [Fact]
    public void ValidRefreshCommand_PassesValidation()
    {
        // arrange
        var model = new Refresh.Command(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidRefreshCommand_FailsValidation()
    {
        // arrange
        var model = new Refresh.Command(null!);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}