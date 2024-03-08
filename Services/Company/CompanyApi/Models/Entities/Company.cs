namespace CompanyApi.Models.Entities;

public sealed class Company
{
    public Guid Id { get; init; }
    public Guid? OwnerId { get; init; }
    public required string Name { get; init; }
    public required string TaxNumber { get; init; }
    public required string City { get; init; }
    public required string Street { get; init; }
    public required string HouseNumber { get; init; }
    public required string PostalCode { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime? Updated { get; set; }
}