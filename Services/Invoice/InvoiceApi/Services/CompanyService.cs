using Contracts;
using MassTransit;

namespace InvoiceApi.Services;

public interface ICompanyService
{
    Task<bool> CompanyExists(Guid id);
}

internal class CompanyService(IRequestClient<GetCompanyRequest> client) : ICompanyService
{
    public async Task<bool> CompanyExists(Guid id)
    {
        var response = await client.GetResponse<CompanyExistResponse>(new GetCompanyRequest { CompanyId = id });
        return response.Message.CompanyExists;
    }
}