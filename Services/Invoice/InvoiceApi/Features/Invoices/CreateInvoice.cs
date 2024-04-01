using Carter;
using Carter.ModelBinding;
using FluentValidation;
using InvoiceApi.Contracts.Invoices;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Domain.Entities.Enums;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Invoices;

public class CreateInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/invoice", async (CreateInvoiceRequest request, HttpContext context, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    SellerCompanyId: request.SellerCompanyId,
                    BuyerCompanyId: request.BuyerCompanyId,
                    TermsOfPayment: request.TermsOfPayment,
                    PaymentType: request.PaymentType,
                    Status: request.Status
                );
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Invoice))
            .Produces<InvoiceResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid SellerCompanyId,
        Guid BuyerCompanyId,
        int TermsOfPayment,
        string PaymentType,
        string Status
    ) : IRequest<Results<Created<InvoiceResponse>, ValidationProblem>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.SellerCompanyId)
                .NotEmpty();

            RuleFor(c => c.BuyerCompanyId)
                .NotEmpty();

            RuleFor(c => c.TermsOfPayment)
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
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Results<Created<InvoiceResponse>, ValidationProblem>>
    {
        public async Task<Results<Created<InvoiceResponse>, ValidationProblem>> Handle(Command commnad, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(commnad, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());

            // todo check company id
            
            var date = DateTime.Now;
            
            var invoiceCount = await dbContext
                .Invoices
                .Where(i => i.OwnerId == commnad.CurrentUserId 
                            && i.Created.Year == date.Year
                            && i.Created.Month == date.Month )
                .CountAsync(ct);

            var invoice = new Invoice
            {
                OwnerId = commnad.CurrentUserId,
                SellerCompanyId = commnad.SellerCompanyId,
                BuyerCompanyId = commnad.BuyerCompanyId,
                Number = $"FV-{++invoiceCount}/{date.Month}/{date.Year}",
                PaymentType = Enum.Parse<Payment>(commnad.PaymentType),
                Status = Enum.Parse<Status>(commnad.Status),
            };
            await dbContext.Invoices.AddAsync(invoice, ct);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Created(invoice.Id.ToString(), new InvoiceResponse(invoice));
        }
    }
}