using FluentValidation;

namespace CompanyApi.Models.Dto;

public sealed record CreateCompanyDto(
    string Name,
    string TaxNumber,
    string City,
    string Street,
    string HouseNumber,
    string PostalCode
);

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyValidator()
    {
        RuleFor(c => c.Name)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(c => c.TaxNumber)
            .NotEmpty()
            .Length(10)
            .Matches(@"^\d+$");

        RuleFor(c => c.City)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.Street)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.HouseNumber)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(c => c.PostalCode)
            .NotEmpty()
            .MaximumLength(10);
    }
}

