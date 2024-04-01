namespace Contracts;

public sealed record GetCompanyRequest
{
    public required Guid CompanyId { get; init; }
}

public sealed record GetCompanyResponse
{
    public required string Name { get; init; }
    public required string TaxNumber { get; init; }
    public required string City { get; init; }
    public required string Street { get; init; }
    public required string HouseNumber { get; init; }
    public required string PostalCode { get; init; }
}

public sealed record GetCompanyNotFoundResponse
{
    public required string Message { get; init; }
}