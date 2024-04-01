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

public class UpdateInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/invoice", async (UpdateInvoiceCommand request, HttpContext context, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    InvoiceId: request.InvoiceId,
                    Status: request.Status
                );
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Invoice))
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid InvoiceId,
        string Status
    ) : IRequest<Results<Ok<InvoiceResponse>, NotFound<string>, ValidationProblem>>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
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

    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Results<Ok<InvoiceResponse>, NotFound<string>, ValidationProblem>>
    {
        public async Task<Results<Ok<InvoiceResponse>, NotFound<string>, ValidationProblem>>
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());

            var invoice = await dbContext
                .Invoices
                .Include(i => i.Items)
                .SingleOrDefaultAsync(i => i.Id == command.InvoiceId, ct);

            if (invoice is null || invoice.OwnerId != command.CurrentUserId)
                return TypedResults.NotFound($"Invoice: {command.InvoiceId} not found or you don't have access");

            invoice.Status = Enum.Parse<Status>(command.Status);

            dbContext.Invoices.Update(invoice);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Ok(new InvoiceResponse(invoice));
        }
    }
}