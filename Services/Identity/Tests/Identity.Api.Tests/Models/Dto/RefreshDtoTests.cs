using System.Security.Cryptography;
using FluentValidation.TestHelper;
using Identity.Api.Models.Dto;
using Xunit;

namespace Identity.Api.Tests.Models.Dto;

public class RefreshDtoTests
{
    private readonly RefreshValidator _validator = new();

    [Fact]
    public void ValidRefreshDto_PassesValidation()
    {
        // arrange
        var model = new RefreshDto(Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)));

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidRefreshDto_FailsValidation()
    {
        // arrange
        var model = new RefreshDto(null!);

        // act
        var result = _validator.TestValidate(model);

        // assert
        result.ShouldHaveAnyValidationError();
    }
}