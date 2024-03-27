using Carter;
using CompanyApi.Contracts;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Domain;
using CompanyApi.Helpers;
using CompanyApi.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public class GetAllCompanies : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/company", async (
                [AsParameters] QueryParameters parameters, HttpContext context, ISender sender) =>
            {
                var query = new Query(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    PageNumber: parameters.PageNumber,
                    PageSize: parameters.PageSize
                );
                return await sender.Send(query);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Company))
            .Produces<PaginatedResponse<CompanyResponse>>();
    }

    public sealed record Query(
        Guid CurrentUserId,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<IResult>;
    
    internal sealed class Handler(AppDbContext dbContext) : IRequestHandler<Query, IResult>
    {
        public async Task<IResult> Handle(Query query, CancellationToken ct)
        {
            var dbQuery = dbContext
                .Companies
                .AsNoTracking()
                .Where(c => c.CreatedByUserId == query.CurrentUserId)
                .AsQueryable();

            var count = await dbQuery
                .CountAsync(ct);
            
            var companyResponses = await dbQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .Select(c => new CompanyResponse(c))
                .ToListAsync(ct);

            return TypedResults
                .Ok(new PaginatedResponse<CompanyResponse>(
                    companyResponses, count, query.PageNumber, query.PageSize));
        }
    }
}
