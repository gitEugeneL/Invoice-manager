namespace InvoiceApi.Models.Dto;

public sealed record QueryParameters(
    int PageNumber = 1,
    int PageSize = 50
);