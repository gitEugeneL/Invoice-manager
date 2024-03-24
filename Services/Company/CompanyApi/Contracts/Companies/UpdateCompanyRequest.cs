namespace CompanyApi.Contracts.Companies;

public sealed record UpdateCompanyRequest(
    Guid CompanyId,
    string? Name,
    string? TaxNumber,
    string? City,
    string? Street,
    string? HouseNumber,
    string? PostalCode
);