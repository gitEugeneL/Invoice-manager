using Identity.Api.Entities.Common;

namespace Identity.Api.Entities;

public sealed class User : BaseAuditableEntity
{
    public required string Email { get; init; }
    public required byte[] PwdHash { get; init; }
    public required byte[] PwdSalt { get; init; } 
    
    /*** Relations ***/
    public List<RefreshToken> RefreshTokens { get; init; } = [];
}