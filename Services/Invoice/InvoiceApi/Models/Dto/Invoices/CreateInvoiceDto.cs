using FluentValidation;
using InvoiceApi.Models.Entities.Enums;

namespace InvoiceApi.Models.Dto.Invoices;

public sealed record CreateInvoiceDto(
    Guid SellerCompanyId,
    Guid BuyerCompanyId,
    int TermsOfPayment,
    string PaymentType,
    string Status
);

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(i => i.SellerCompanyId)
            .NotEmpty();

        RuleFor(i => i.BuyerCompanyId)
            .NotEmpty();

        RuleFor(i => i.TermsOfPayment)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100);

        RuleFor(i => i.PaymentType)
            .NotNull()
            .Must(pt => Enum.IsDefined(typeof(Payment), pt))
            .WithMessage(
                $"Payment type is not valid. Valid types are: {string.Join(", ", Enum.GetNames(typeof(Payment)))}");
        
        RuleFor(i => i.Status)
            .NotNull()
            .Must(st => Enum.IsDefined(typeof(Status), st))
            .WithMessage(
                $"Status type is not valid. Valid types are: {string.Join(", ", Enum.GetNames(typeof(Status)))}");
    }
} 