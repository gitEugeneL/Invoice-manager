using Carter;
using InvoiceApi.Contracts;
using InvoiceApi.Contracts.Invoices;
using InvoiceApi.Data;
using InvoiceApi.Helpers;
using InvoiceApi.Models.Entities;
using InvoiceApi.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Invoices;

public class GetAllInvoices : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/invoice", async (
                [AsParameters] QueryParameters parameters, HttpContext context, ISender sender) =>
            {
                var query = new Query(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    PageNumber: parameters.PageNumber,
                    PageSize: parameters.PageSize
                );
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Invoice))
            .Produces<List<InvoiceResponse>>();
    }

    public sealed record Query(
        Guid CurrentUserId,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<IResult>;

    internal sealed class Handler(AppDbContext dbContext) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query query, CancellationToken ct)
        {
            var dbQuery = dbContext
                .Invoices
                .Include(i => i.Items)
                .AsNoTracking()
                .Where(i => i.OwnerId == query.CurrentUserId)
                .AsQueryable();
            
            var count = await dbQuery
                .CountAsync(ct);
            
            var invoiceResponses = await dbQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .Select(i => new InvoiceResponse(i))
                .ToListAsync(ct);

            return TypedResults
                .Ok(new PaginatedResponse<InvoiceResponse>(
                    invoiceResponses, count, query.PageNumber, query.PageSize));
        }
    }
}