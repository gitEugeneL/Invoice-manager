using Carter;
using InvoiceApi.Data;
using InvoiceApi.Helpers;
using InvoiceApi.Models.Entities;
using InvoiceApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApi.Features.Invoices;

public class LockInvoice : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/v1/invoice/lock/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var command = new Command(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Invoice))
            .Produces<Guid>()
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid InvoiceId    
    ) : IRequest<Results<Ok<Guid>, NotFound<string>>>;
    
    internal sealed class Handler(AppDbContext dbContext) : IRequestHandler<Command, Results<Ok<Guid>, NotFound<string>>>
    {
        public async Task<Results<Ok<Guid>, NotFound<string>>> Handle(Command command, CancellationToken ct)
        {
            var invoice = await dbContext
                .Invoices
                .SingleOrDefaultAsync(i => i.Id == command.InvoiceId 
                                           && i.OwnerId == command.CurrentUserId, ct);
            
            if (invoice is null)
                return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");

            invoice.Locked = true;

            dbContext.Invoices.Update(invoice);
            await dbContext.SaveChangesAsync(ct);
            
            return TypedResults.Ok(invoice.Id);
        }
    }
}