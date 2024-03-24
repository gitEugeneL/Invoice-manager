namespace CompanyApi.Contracts.Companies;

public sealed record CreateCompanyRequest(
    string Name, 
    string TaxNumber, 
    string City, 
    string Street, 
    string HouseNumber, 
    string PostalCode
);