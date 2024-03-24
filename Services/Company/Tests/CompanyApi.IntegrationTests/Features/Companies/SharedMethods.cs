using CompanyApi.Contracts.Companies;

namespace CompanyApi.IntegrationTests.Features.Companies;

public static class SharedMethods
{
    public static async Task<CompanyResponse> CreateCompany(HttpClient client)
    {
        var model = new CreateCompanyRequest(
            "Company", "5555555555", "Radom", "Warszawska", "123", "02-495");
        var response = await client.PostAsync("api/v1/company", TestCase.CreateContext(model));
        return await TestCase.DeserializeResponse<CompanyResponse>(response);
    }
}