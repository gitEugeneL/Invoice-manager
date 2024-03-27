using IdentityApi.Domain.Entities;

namespace IdentityApi.Security.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(User user);
}