using Identity.Api.Models.Entities;

namespace Identity.Api.Models.Dto;

public sealed class UserResponseDto()
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;

    public UserResponseDto(User user) : this()
    {
        UserId = user.Id;
        Email = user.Email;
    }
}