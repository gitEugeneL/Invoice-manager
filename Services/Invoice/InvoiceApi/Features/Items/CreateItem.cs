using Carter;
using Carter.ModelBinding;
using FluentValidation;
using InvoiceApi.Contracts.Items;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Domain.Entities.Enums;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Unit = InvoiceApi.Domain.Entities.Enums.Unit;

namespace InvoiceApi.Features.Items;

public class CreateItem : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/item", async (CreateItemCommand request, HttpContext context, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    InvoiceId: request.InvoiceId,
                    Name: request.Name,
                    Amount: request.Amount,
                    Unit: request.Unit,
                    Vat: request.Vat,
                    NetPrice: request.NetPrice
                );
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Item))
            .Produces<ItemResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid InvoiceId,
        string Name,
        int Amount,
        string Unit,
        string Vat,
        decimal NetPrice
    ) : IRequest<Results<Created<ItemResponse>, NotFound<string>, ValidationProblem>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
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
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Results<Created<ItemResponse>, NotFound<string>, ValidationProblem>>
    {
        public async Task<Results<Created<ItemResponse>, NotFound<string>, ValidationProblem>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());

            var invoice = await dbContext
                .Invoices
                .SingleOrDefaultAsync(inv => inv.Id == command.InvoiceId
                                             && inv.OwnerId == command.CurrentUserId
                                             && inv.Locked == false, ct);
            if (invoice is null)
                return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");

            var item = new Item
            {
                Name = command.Name,
                Amount = command.Amount,
                Unit = Enum.Parse<Unit>(command.Unit),
                Vat = Enum.Parse<Vat>(command.Vat),
                NetPrice = command.NetPrice,
                Invoice = invoice
            };

            await dbContext.Items.AddAsync(item, ct);
            await dbContext.SaveChangesAsync(ct);
            
            return TypedResults.Created(item.Id.ToString(), new ItemResponse(item));
        }
    }
}