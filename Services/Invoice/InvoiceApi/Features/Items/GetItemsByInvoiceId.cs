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

public class GetItemsByInvoiceId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/item/all-by-invoice/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new Query(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Item))
            .Produces<List<ItemResponse>>()
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Query(
        Guid CurrentUserId,
        Guid InvoiceId
    ) : IRequest<Results<Ok<List<ItemResponse>>, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext dbContext
    ) : IRequestHandler<Query, Results<Ok<List<ItemResponse>>, NotFound<string>>>
    {
        public async Task<Results<Ok<List<ItemResponse>>, NotFound<string>>> Handle(Query query, CancellationToken ct)
        {
            var itemResponses = await dbContext
                .Items
                .AsNoTracking()
                .Include(i => i.Invoice)
                .Where(i => i.Invoice.Id == query.InvoiceId
                            && i.Invoice.OwnerId == query.CurrentUserId)
                .Select(i => new ItemResponse(i))
                .ToListAsync(ct);

            return TypedResults.Ok(itemResponses);
        }
    }
}