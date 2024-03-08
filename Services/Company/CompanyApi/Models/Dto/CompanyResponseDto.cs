using CompanyApi.Models.Entities;

namespace CompanyApi.Models.Dto;

public sealed class CompanyResponseDto()
{
    public string Name { get; init; } = string.Empty;
    public string TaxNumber { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string HouseNumber { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;

    public CompanyResponseDto(Company company) : this()
    {
        Name = company.Name;
        TaxNumber = company.TaxNumber;
        City = company.City;
        Street = company.Street;
        HouseNumber = company.HouseNumber;
        PostalCode = company.PostalCode;
    }
}