using FluentValidation;

namespace Identity.Api.Models.Dto;

public sealed record LoginDto(
    string Email,
    string Password
);

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(l => l.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}

