using FluentValidation;

namespace InvoiceApi.Models.Dto.Items;

public sealed record UpdateItemDto(
    Guid ItemId,
    string? Name,
    int? Amount,
    decimal? NetPrice
);

public sealed class UpdateItemValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemValidator()
    {
        RuleFor(i => i.ItemId)
            .NotEmpty();

        RuleFor(i => i.Name)
            .MaximumLength(100);
        
        RuleFor(i => i.Amount)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(999999);
        
        RuleFor(i => i.NetPrice)
            .GreaterThanOrEqualTo(1);

    }
}