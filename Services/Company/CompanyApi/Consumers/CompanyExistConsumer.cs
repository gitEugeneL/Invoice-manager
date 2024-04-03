using CompanyApi.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Consumers;

public class CompanyExistConsumer(AppDbContext dbContext) : IConsumer<CompanyExistRequest>
{
    public async Task Consume(ConsumeContext<CompanyExistRequest> context)
    {
        var result = await dbContext
            .Companies
            .AsNoTracking()
            .AnyAsync(company => company.Id == context.Message.CompanyId);

        await context.RespondAsync(new CompanyExistResponse { CompanyExists = result });
    }
}