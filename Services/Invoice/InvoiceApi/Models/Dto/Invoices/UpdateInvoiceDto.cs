using FluentValidation;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Dto.Invoices;

public sealed record UpdateInvoiceDto(
    Guid InvoiceId,
    string Status
);

public sealed class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(i => i.InvoiceId)
            .NotEmpty();
        
        RuleFor(i => i.Status)
            .NotNull()
            .Must(st => Enum.IsDefined(typeof(Status), st))
            .WithMessage(
                $"Status type is not valid. Valid types are: {string.Join(", ", Enum.GetNames(typeof(Status)))}");
    }
}