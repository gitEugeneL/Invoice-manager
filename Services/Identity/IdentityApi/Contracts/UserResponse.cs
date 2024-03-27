using IdentityApi.Domain.Entities;

namespace IdentityApi.Contracts;

public sealed class UserResponse()
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;

    public UserResponse(User user) : this()
    {
        UserId = user.Id;
        Email = user.Email;
    }
}