using Carter;
using InvoiceApi.Data;
using InvoiceApi.Helpers;
using InvoiceApi.Models.Entities;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Invoices;

public class DeleteInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/invoice/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var command = new Command(TokenService.ReadUserIdFromToken(context),  id);
                return await sender.Send(command);
            })
        .RequireAuthorization(AppConstants.BaseAuthPolicy)
        .WithTags(nameof(Invoice))
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid InvoiceId
    ) : IRequest<Results<NoContent, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext dbContext
    ) : IRequestHandler<Command, Results<NoContent, NotFound<string>>>
    {
        public async Task<Results<NoContent, NotFound<string>>> Handle(Command commnad, CancellationToken ct)
        {
            var invoice = await dbContext
                .Invoices
                .SingleOrDefaultAsync(i => i.Id == commnad.InvoiceId 
                                           && i.OwnerId == commnad.CurrentUserId, ct);
            if (invoice is null)
                return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");

            dbContext.Remove(invoice);
            await dbContext.SaveChangesAsync(ct);
            
            return TypedResults.NoContent();
        }
    }
}