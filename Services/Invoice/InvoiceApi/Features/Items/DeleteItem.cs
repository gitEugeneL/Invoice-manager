using Carter;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Items;

public class DeleteItem : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/item/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var command = new Command(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(command);
            })
        .RequireAuthorization(AppConstants.BaseAuthPolicy)
        .WithTags(nameof(Item))
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status204NoContent);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid ItemId
    ) : IRequest<Results<NoContent, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext dbContext
    ) : IRequestHandler<Command, Results<NoContent, NotFound<string>>>
    {
        public async Task<Results<NoContent, NotFound<string>>> Handle(Command command, CancellationToken ct)
        {
            var item = await dbContext
                .Items
                .Include(i => i.Invoice)
                .SingleOrDefaultAsync(i => i.Id == command.ItemId
                                           && i.Invoice.OwnerId == command.CurrentUserId
                                           && i.Invoice.Locked == false, ct);
            if (item is null)
                return TypedResults.NotFound($"Item: {command.ItemId} not found or you don't have access");

            dbContext.Items.Remove(item);
            await dbContext.SaveChangesAsync(ct);
            
            return TypedResults.NoContent();
        }
    }
}