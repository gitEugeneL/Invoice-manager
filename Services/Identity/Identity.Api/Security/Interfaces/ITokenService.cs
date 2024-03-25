using Identity.Api.Entities;

namespace Identity.Api.Security.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user);
}