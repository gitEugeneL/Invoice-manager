using CompanyApi.Domain.Entities;

namespace CompanyApi.Contracts.Companies;

public sealed class CompanyResponse()
{
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string HouseNumber { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;

    public CompanyResponse(Company company): this()
    {
        CompanyId = company.Id;
        Name = company.Name;
        TaxNumber = company.TaxNumber;
        City = company.City;
        Street = company.Street;
        HouseNumber = company.HouseNumber;
        PostalCode = company.PostalCode;
    }
}