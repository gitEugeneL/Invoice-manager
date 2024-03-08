using CompanyApi.Data;
using CompanyApi.Models.Entities;
using CompanyApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Repositories;

internal class CompanyRepository(AppDbContext context) : ICompanyRepository
{
    public async Task<Company> CreateCompany(Company company)
    {
        await context
            .Companies
            .AddAsync(company);
        await context
            .SaveChangesAsync();
        return company;
    }

    public async Task<Company> UpdateCompany(Company company)
    {
        context
            .Companies
            .Update(company);
        await context
            .SaveChangesAsync();
        return company;
    }

    public async Task<Company?> FindCompanyById(Guid companyId)
    {
        return await context
            .Companies
            .SingleOrDefaultAsync(c => c.Id == companyId);
    }

    public async Task DeleteCompany(Company company)
    {
        context
            .Companies
            .Remove(company);
        await context
            .SaveChangesAsync();
    }

    public async Task<(IEnumerable<Company> List, int Count)> FindCompaniesByOwnerId(
        Guid ownerId, 
        int pageNumber, 
        int pageSize)
    {
        var query = context
            .Companies
            .Where(c => c.CreatedByUserId == ownerId)
            .AsQueryable();

        var count = await query
            .CountAsync();

        var companies = await query
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();

        return (companies, count);
    }
}