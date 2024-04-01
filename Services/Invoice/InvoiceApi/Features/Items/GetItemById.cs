using Carter;
using InvoiceApi.Contracts.Items;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Items;

public class GetItemById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/item/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new Query(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Item))
            .Produces<ItemResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Query(
        Guid CurrentUserId,
        Guid ItemId
    ) : IRequest<Results<Ok<ItemResponse>, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext appDbContext
    ) : IRequestHandler<Query, Results<Ok<ItemResponse>, NotFound<string>>>
    {
        public async Task<Results<Ok<ItemResponse>, NotFound<string>>> Handle(Query query, CancellationToken ct)
        {
            var itemResponse = await appDbContext
                .Items
                .AsNoTracking()
                .Include(i => i.Invoice)
                .Where(i => i.Id == query.ItemId
                            && i.Invoice.OwnerId == query.CurrentUserId)
                .Select(i => new ItemResponse(i))
                .SingleOrDefaultAsync(ct);
            
            return itemResponse is not null
                ? TypedResults.Ok(itemResponse)
                : TypedResults.NotFound($"Item: {query.ItemId} not found or you don't have access");
        }
    }
}