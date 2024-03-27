namespace CompanyApi.Domain;

public sealed class Company
{
    public Guid Id { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required string Name { get; set; }
    public required string TaxNumber { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string HouseNumber { get; set; }
    public required string PostalCode { get; set; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime? Updated { get; set; }
}