using CompanyApi.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Consumers;

public class GetCompanyConsumer(AppDbContext dbContext) : IConsumer<GetCompanyRequest>
{
    public async Task Consume(ConsumeContext<GetCompanyRequest> context)
    {
        var company = await dbContext
            .Companies
            .AsNoTracking()
            .Where(company => company.Id == context.Message.CompanyId)
            .Select(company => new GetCompanyResponse
            {
                Name = company.Name,
                TaxNumber = company.TaxNumber,
                City = company.City,
                Street = company.Street,
                HouseNumber = company.HouseNumber,
                PostalCode = company.PostalCode
            })
            .FirstOrDefaultAsync();
        
        if (company is null)
            await context.RespondAsync(
                new GetCompanyNotFoundResponse { Message = $"company {context.Message.CompanyId} not found" });
        else
            await context.RespondAsync(company);
    }
}