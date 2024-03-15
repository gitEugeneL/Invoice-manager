using InvoiceApi.Models.Dto.Items;
using InvoiceApi.Models.Entities;
using InvoiceApi.Models.Entities.Enums;
using InvoiceApi.Repositories.Interfaces;
using InvoiceApi.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApi.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/v1/item")
            .WithTags("Item")
            .RequireAuthorization("base-policy");

        group.MapPost("", CreateItem)
            .WithValidator<CreateItemDto>()
            .Produces<ItemResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("{itemId:guid}", GetItemById)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ItemResponseDto>();

        group.MapGet("all-by-invoice/{invoiceId:guid}", GetItemsByInvoiceId)
            .Produces<List<ItemResponseDto>>();

        group.MapDelete("{itemId:guid}", DeleteItem)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status204NoContent);
    }
    
    private static async Task<Results<Ok<ItemResponseDto>, NotFound<string>>> CreateItem(
        [FromBody] CreateItemDto dto,
        HttpContext httpContext,
        IInvoiceRepository invoiceRepository,
        IItemRepository itemRepository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var invoice = await invoiceRepository.FindInvoiceById(dto.InvoiceId);
        if (invoice is null || invoice.OwnerId != userId || invoice.Locked)
            return TypedResults.NotFound($"Invoice: {invoice} not found or you don't have access");
        
        var item = await itemRepository.CreateItem(
            new Item 
            {
                Name = dto.Name,
                Amount = dto.Amount,
                Unit = Enum.Parse<Unit>(dto.Unit),
                Vat = Enum.Parse<Vat>(dto.Vat),
                NetPrice = dto.NetPrice,
                Invoice = invoice
            }
        );
        return TypedResults.Ok(new ItemResponseDto(item));
    }

    private static async Task<Results<Ok<ItemResponseDto>, NotFound<string>>> GetItemById(
        Guid itemId,
        HttpContext httpContext,
        IItemRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var item = await repository.FindItemById(itemId);
        
        return item is null || item.Invoice.OwnerId != userId
            ? TypedResults.NotFound($"Item: {itemId} not found or you don't have access")
            : TypedResults.Ok(new ItemResponseDto(item));
    }

    private static async Task<IResult> GetItemsByInvoiceId(
        Guid invoiceId,
        HttpContext httpContext,
        IItemRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var items = await repository.FindAllByInvoiceIdForOwner(invoiceId, userId);
        var result = items
            .Select(i => new ItemResponseDto(i))
            .ToList();
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ItemResponseDto>, NotFound<string>>> UpdateItem(
        [FromBody] UpdateItemDto dto,
        HttpContext httpContext,
        IItemRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var item = await repository.FindItemById(dto.ItemId);
        if (item is null || item.Invoice.OwnerId != userId || item.Invoice.Locked)
            TypedResults.NotFound($"Item: {dto.ItemId} not found or you don't have access");

        item.Name = dto.Name ?? item.Name;
        item.Amount = dto.Amount ?? item.Amount;
        item.NetPrice = dto.NetPrice ?? item.NetPrice;

        await repository.UpdateItem(item);
        return TypedResults.Ok(new ItemResponseDto(item));
    }
    
    private static async Task<Results<NoContent, NotFound<string>>> DeleteItem(
        Guid itemId,
        HttpContext httpContext,
        IItemRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var item = await repository.FindItemById(itemId);
        if (item is null || item.Invoice.OwnerId != userId || item.Invoice.Locked)
            return TypedResults.NotFound($"Item: {itemId} not found or you don't have access");

        await repository.DeleteItem(item);
        return TypedResults.NoContent();
    }
}