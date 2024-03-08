using FluentValidation;

namespace CompanyApi.Models.Dto;

public sealed record UpdateCompanyDto(
    Guid CompanyId,
    string? Name,
    string? TaxNumber,
    string? City,
    string? Street,
    string? HouseNumber,
    string? PostalCode
);

public sealed class UpdateCompanyValidator : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyValidator()
    {
        RuleFor(c => c.CompanyId)
            .NotEmpty();
        
        RuleFor(c => c.Name)
            .MaximumLength(100);

        RuleFor(c => c.TaxNumber)
            .Length(10)
            .Matches(@"^\d+$");

        RuleFor(c => c.City)
            .MaximumLength(50);

        RuleFor(c => c.Street)
            .MaximumLength(50);

        RuleFor(c => c.HouseNumber)
            .MaximumLength(10);

        RuleFor(c => c.PostalCode)
            .MaximumLength(10);
    }
}
