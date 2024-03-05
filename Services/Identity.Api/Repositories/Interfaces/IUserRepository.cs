using Identity.Api.Entities;

namespace Identity.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task CreateUser(User user);
    Task UpdateUser(User user);
    Task<User?> FindUserById(Guid id);
    Task<User?> FindUserByEmail(string email);
    Task<User?> FindUserByRefreshToken(string refreshToken);
}