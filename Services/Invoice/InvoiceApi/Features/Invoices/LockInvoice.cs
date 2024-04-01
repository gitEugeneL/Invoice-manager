using Carter;
using Contracts;
using InvoiceApi.Data;
using InvoiceApi.Domain.Entities;
using InvoiceApi.Helpers;
using InvoiceApi.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Item = Contracts.Item;

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
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint
    ) : IRequestHandler<Command, Results<Ok<Guid>, NotFound<string>>>
    {
        public async Task<Results<Ok<Guid>, NotFound<string>>> Handle(Command command, CancellationToken ct)
        {
            var invoice = await dbContext
                .Invoices
                .Include(i => i.Items)
                .SingleOrDefaultAsync(i => i.Id == command.InvoiceId 
                                           && i.OwnerId == command.CurrentUserId 
                                           && i.Locked == false
                                           && i.Items.Count > 0, ct);
            if (invoice is null)
                return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");
            
            invoice.Locked = true;
            
            dbContext.Invoices.Update(invoice);
            await dbContext.SaveChangesAsync(ct);
            
            await publishEndpoint.Publish(
                new FileCreateEvent
                { 
                    InvoiceId = invoice.Id, 
                    OwnerId = command.CurrentUserId,
                    SellerCompanyId = invoice.SellerCompanyId, 
                    BuyerCompanyId = invoice.BuyerCompanyId,
                    Number = invoice.Number,
                    TotalNetPrice = invoice.TotalNetPrice,
                    TotalGrossPrice = invoice.TotalGrossPrice,
                    TermsOfPayment = invoice.TermsOfPayment,
                    PaymentType = invoice.PaymentType.ToString(),
                    Status = invoice.Status.ToString(),
                    Items = invoice.Items.Select(item  => new Item
                    {
                        ItemId = item.Id,
                        Name = item.Name,
                        Amount = item.Amount,
                        Unit = item.Unit.ToString(),
                        Vat = item.Vat.ToString(),
                        NetPrice = item.NetPrice,
                        SumNetPrice = item.SumNetPrice,
                        SumGrossPrice = item.GrossPrice
                    })
                }, ct);
            
            return TypedResults.Ok(invoice.Id);
        }
    }
}