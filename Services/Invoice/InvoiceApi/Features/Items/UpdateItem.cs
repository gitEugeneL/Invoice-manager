using Carter;
using Carter.ModelBinding;
using FluentValidation;
using InvoiceApi.Contracts.Items;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Items;

public class UpdateItem : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/item", async (UpdateItemRequest request, HttpContext context, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    ItemId: request.ItemId,
                    Name: request.Name,
                    Amount: request.Amount,
                    NetPrice: request.NetPrice
                );
                return await sender.Send(command);
            })
        .RequireAuthorization(AppConstants.BaseAuthPolicy)
        .WithTags(nameof(Item))
        .Produces<ItemResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid ItemId,
        string? Name,
        int? Amount,
        decimal? NetPrice
            
    ) : IRequest<Results<Ok<ItemResponse>, ValidationProblem, NotFound<string>>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
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
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Results<Ok<ItemResponse>, ValidationProblem, NotFound<string>>>
    {
        public async Task<Results<Ok<ItemResponse>, ValidationProblem, NotFound<string>>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());

            var item = await dbContext
                .Items
                .Include(i => i.Invoice)
                .SingleOrDefaultAsync(i => i.Id == command.ItemId 
                                      && i.Invoice.OwnerId == command.CurrentUserId 
                                      && i.Invoice.Locked == false, ct);

            if (item is null)
                return TypedResults.NotFound($"Item: {command.ItemId} not found or you don't have access");

            item.Name = command.Name ?? item.Name;
            item.Amount = command.Amount ?? item.Amount;
            item.NetPrice = command.NetPrice ?? item.NetPrice;

            dbContext.Items.Update(item);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Ok(new ItemResponse(item));
        }
    }
}