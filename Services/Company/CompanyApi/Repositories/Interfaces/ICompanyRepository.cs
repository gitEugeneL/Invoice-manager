using CompanyApi.Models.Entities;

namespace CompanyApi.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company> CreateCompany(Company company);
    Task<Company> UpdateCompany(Company company);
    Task<Company?> FindCompanyById(Guid companyId);
    Task DeleteCompany(Company company);
    Task<(IEnumerable<Company> List, int Count)> FindCompaniesByOwnerId(Guid ownerId, int pageNumber, int pageSize);
}