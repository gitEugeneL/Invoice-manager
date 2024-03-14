using FluentValidation;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Dto.Items;

public sealed record CreateItemDto(
    Guid InvoiceId,
    string Name,
    int Amount,
    string Unit,
    string Vat,
    decimal NetPrice
);

public sealed class CreateItemValidator : AbstractValidator<CreateItemDto>
{
    public CreateItemValidator()
    {
        RuleFor(i => i.InvoiceId)
            .NotEmpty();
        
        RuleFor(i => i.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(i => i.Amount)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(999999);

        RuleFor(i => i.Unit)
            .NotNull()
            .Must(un => Enum.IsDefined(typeof(Unit), un))
            .WithMessage(
                $"Unit type is not valid. Valid types are: {string.Join(", ", Enum.GetNames(typeof(Unit)))}");

        RuleFor(i => i.Vat)
            .NotNull()
            .Must(va => Enum.IsDefined(typeof(Vat), va))
            .WithMessage(
                $"Vat type is not valid. Valid types are: {string.Join(", ", Enum.GetNames(typeof(Vat)))}");

        RuleFor(i => i.NetPrice)
            .NotEmpty()
            .GreaterThanOrEqualTo(1);
    }
}