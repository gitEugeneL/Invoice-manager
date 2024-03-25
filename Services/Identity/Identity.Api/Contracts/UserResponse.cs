using Identity.Api.Entities;

namespace Identity.Api.Contracts;

public sealed record UserResponse(Guid UserId, string Email)
{
    public UserResponse(User user) : this(user.Id, user.Email) { }
}