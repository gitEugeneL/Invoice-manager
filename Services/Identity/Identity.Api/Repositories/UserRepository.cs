using Identity.Api.Data;
using Identity.Api.Models.Entities;
using Identity.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Repositories;

internal class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task CreateUser(User user)
    {
        await context
            .Users
            .AddAsync(user);
        
        await context
            .SaveChangesAsync();
    }

    public async Task UpdateUser(User user)
    {
        context
            .Users
            .Update(user);
        
        await context
            .SaveChangesAsync();
    }

    public async Task<User?> FindUserById(Guid id)
    {
        return await context
            .Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> FindUserByEmail(string email)
    {
        return await context
            .Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email.ToLower()
                .Equals(email.ToLower()));
    }

    public async Task<User?> FindUserByRefreshToken(string refreshToken)
    {
        return await context
            .Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.RefreshTokens
                .Any(rt => rt.Token == refreshToken));
    }
}