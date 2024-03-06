using FluentValidation;

namespace Identity.Api.Models.Dto;

public sealed record RefreshDto(string RefreshToken);

public sealed class RefreshValidator : AbstractValidator<RefreshDto>
{
    public RefreshValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty();
    }
}