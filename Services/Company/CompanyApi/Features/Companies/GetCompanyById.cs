using Carter;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Domain;
using CompanyApi.Helpers;
using CompanyApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public class GetCompanyById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/company/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new Query(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    CompanyId: id
                );
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Company))
            .Produces<CompanyResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public sealed record Query(
        Guid CurrentUserId,
        Guid CompanyId
    ) : IRequest<Results<Ok<CompanyResponse>, NotFound<string>>>;
    
    internal sealed class Handler(
        AppDbContext dbContext
    ) : IRequestHandler<Query, Results<Ok<CompanyResponse>, NotFound<string>>>
    {
        public async Task<Results<Ok<CompanyResponse>, NotFound<string>>> Handle(Query query, CancellationToken ct)
        {
            var companyResponse = await dbContext
                .Companies
                .AsNoTracking()
                .Where(c => c.Id == query.CompanyId && c.CreatedByUserId == query.CurrentUserId)
                .Select(company => new CompanyResponse(company))
                .SingleOrDefaultAsync(ct);

            return companyResponse is not null
                ? TypedResults.Ok(companyResponse)
                : TypedResults.NotFound($"Company {query.CompanyId} not found");
        }
    }
}
