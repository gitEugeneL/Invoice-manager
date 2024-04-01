using Contracts;
using MassTransit;

namespace FileGeneratorApi.Services;

public interface ICompanyService
{
    Task<GetCompanyResponse> GetCompanyAsync(Guid id);
}

internal class CompanyService(IRequestClient<GetCompanyRequest> client) : ICompanyService
{
    public async Task<GetCompanyResponse> GetCompanyAsync(Guid id)
    {
        var response =  await client.GetResponse<GetCompanyResponse, GetCompanyNotFoundResponse>(
            new GetCompanyRequest { CompanyId = id });
        
        return response.Is(out Response<GetCompanyResponse> companyResponse)
            ? companyResponse.Message
            : throw new Exception("company not found");
    }
}