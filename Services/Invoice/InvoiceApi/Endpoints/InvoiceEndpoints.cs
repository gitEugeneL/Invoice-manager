using InvoiceApi.Models.Dto;
using InvoiceApi.Models.Dto.Invoices;
using InvoiceApi.Models.Entities;
using InvoiceApi.Models.Entities.Enums;
using InvoiceApi.Repositories.Interfaces;
using InvoiceApi.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApi.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/v1/invoice")
            .WithTags("Invoice")
            .RequireAuthorization("base-policy");

            group.MapPost("", CreateInvoice)
                .WithValidator<CreateInvoiceDto>()
                .Produces<InvoiceResponseDto>(StatusCodes.Status201Created);
            
            group.MapPut("", UpdateInvoice)
                .WithValidator<UpdateInvoiceDto>()
                .Produces<InvoiceResponseDto>()
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("{invoiceId:guid}", DeleteInvoice)
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);
            
            group.MapGet("", GetAllInvoices)
                .Produces<PaginatedResponse<InvoiceResponseDto>>();

            group.MapPatch("lock/{invoiceId:guid}", LockInvoice)
                .Produces<Guid>()
                .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateInvoice(
        [FromBody] CreateInvoiceDto dto,
        HttpContext httpContext,
        IInvoiceRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var date = DateTime.Now;
        var count = await repository.CountInvoicesByMonthAndOwnerId(date, userId);
        
        var invoice = await repository.CreateInvoice(
            new Invoice
            {
                OwnerId = userId,
                SellerCompanyId = dto.SellerCompanyId,
                BuyerCompanyId = dto.BuyerCompanyId,
                Number = $"FV-{++count}/{date.Month}/{date.Year}",
                PaymentType = Enum.Parse<Payment>(dto.PaymentType),
                Status = Enum.Parse<Status>(dto.Status),
            }
        );
        return TypedResults.Created(invoice.Id.ToString(), new InvoiceResponseDto(invoice));
    }

    private static async Task<Results<Ok<InvoiceResponseDto>, NotFound<string>>> UpdateInvoice(
        [FromBody] UpdateInvoiceDto dto,
        HttpContext httpContext,
        IInvoiceRepository repository
        )
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var invoice = await repository.FindInvoiceById(dto.InvoiceId);
        if (invoice is null || invoice.OwnerId != userId)
            return TypedResults.NotFound($"Invoice: {dto.InvoiceId} not found or you don't have access");

        invoice.Status = Enum.Parse<Status>(dto.Status);
        await repository.UpdateInvoice(invoice);
        return TypedResults.Ok(new InvoiceResponseDto(invoice));
    }

    private static async Task<Results<NoContent, NotFound<string>>> DeleteInvoice(
        Guid invoiceId,
        HttpContext httpContext,
        IInvoiceRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var invoice = await repository.FindInvoiceById(invoiceId);
        if (invoice is null || invoice.OwnerId != userId)
            return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");

        await repository.DeleteInvoice(invoice);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAllInvoices(
        [AsParameters] QueryParameters parameters,
        HttpContext httpContext,
        IInvoiceRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var (invoices, count) =
            await repository.FindInvoicesByOwnerId(userId, parameters.PageNumber, parameters.PageSize);

        return TypedResults
            .Ok(new PaginatedResponse<InvoiceResponseDto>(
                invoices
                    .Select(i => new InvoiceResponseDto(i))
                    .ToList(),
                count,
                parameters.PageNumber,
                parameters.PageSize));
    }

    private static async Task<Results<Ok, NotFound<string>>> LockInvoice(
        Guid invoiceId,
        HttpContext httpContext,
        IInvoiceRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var invoice = await repository.FindInvoiceById(invoiceId);
        if (invoice is null || invoice.OwnerId != userId)
            return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");

        invoice.Locked = true;
        await repository.UpdateInvoice(invoice);
        return TypedResults.Ok();
    }
}