namespace CompanyApi.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/v1/company")
            .WithTags("Company");
    }
}