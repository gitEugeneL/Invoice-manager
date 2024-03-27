using Carter;
using InvoiceApi.Contracts.Invoices;
using InvoiceApi.Data;
using InvoiceApi.Helpers;
using InvoiceApi.Models.Entities;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Invoices;

public class GetInvoiceById : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/invoice/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new Query(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Invoice))
            .Produces<InvoiceResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Query(
        Guid CurrentUserId,
        Guid InvoiceId
    ) : IRequest<Results<Ok<InvoiceResponse>, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext dbContext
    ) : IRequestHandler<Query, Results<Ok<InvoiceResponse>, NotFound<string>>>
    {
        public async Task<Results<Ok<InvoiceResponse>, NotFound<string>>> Handle(Query query, CancellationToken ct)
        {
            var invoice = await dbContext
                .Invoices
                .AsNoTracking()
                .Include(i => i.Items)
                .SingleOrDefaultAsync(i => i.Id == query.InvoiceId 
                                           && i.OwnerId == query.CurrentUserId, ct);

            return invoice is not null
                ? TypedResults.Ok(new InvoiceResponse(invoice))
                : TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");
        }
    }
}