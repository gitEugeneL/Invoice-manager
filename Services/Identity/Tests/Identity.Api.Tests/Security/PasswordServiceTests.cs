using IdentityApi.Security;
using Xunit;

namespace Identity.Api.Tests.Security;

public class PasswordServiceTests
{
    [Fact]
    public void CreatePasswordHash_ValidPassword_GeneratesHashAndSalt()
    {
        // arrange
        var passwordManager = new PasswordService();
        const string password = "TestPassword";

        // act
        passwordManager.CreatePasswordHash(password, out var hash, out var salt);

        // assert
        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);
    }

    [Fact]
    public void VerifyPasswordHash_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var passwordManager = new PasswordService();
        const string password = "TestPassword";
        passwordManager.CreatePasswordHash(password, out var hash, out var salt);

        // Act
        var result = passwordManager.VerifyPasswordHash(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPasswordHash_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var passwordManager = new PasswordService();
        const string password = "TestPassword";
        const string incorrectPassword = "IncorrectPassword";
        passwordManager.CreatePasswordHash(password, out var hash, out var salt);

        // Act
        var result = passwordManager.VerifyPasswordHash(incorrectPassword, hash, salt);

        // Assert
        Assert.False(result);
    }
}
