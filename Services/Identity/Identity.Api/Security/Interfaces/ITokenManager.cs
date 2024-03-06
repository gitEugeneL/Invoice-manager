using Identity.Api.Models.Entities;

namespace Identity.Api.Security.Interfaces;

public interface ITokenManager
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user);
}