using InvoiceApi.Services;

namespace InvoiceApi.IntegrationTests.Features;

public class FakeCompanyService : ICompanyService
{
    public Task<bool> CompanyExists(Guid id)
    {
        return Task.FromResult(true);
    }
}