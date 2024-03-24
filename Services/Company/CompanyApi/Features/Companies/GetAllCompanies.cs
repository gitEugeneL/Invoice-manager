using Carter;
using CompanyApi.Contracts;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public static class GetAllCompanies
{
    public sealed class Query : IRequest<Result<PaginatedResponse<CompanyResponse>>>
    {
        public Guid CurrentUserId { get; init; }
        
        public int PageNumber { get; init; }
        
        public int PageSize { get; init; }
    }
    
    internal sealed class Handler(AppDbContext dbContext) 
        : IRequestHandler<Query, Result<PaginatedResponse<CompanyResponse>>>
    {
        public async Task<Result<PaginatedResponse<CompanyResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = dbContext
                .Companies
                .AsNoTracking()
                .Where(c => c.CreatedByUserId == request.CurrentUserId)
                .AsQueryable();

            var count = await query
                .CountAsync(cancellationToken);

            var companyResponses = await query
                .Skip(request.PageSize * (request.PageNumber - 1))
                .Take(request.PageSize)
                .Select(c => new CompanyResponse(c))
                .ToListAsync(cancellationToken);
            
            return new PaginatedResponse<CompanyResponse>(
                companyResponses, count, request.PageNumber, request.PageSize);
        }
    }
}

public class GetAllCompaniesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/company", async (
                [AsParameters] QueryParameters parameters, HttpContext context, ISender sender) =>
            {
                var query = new GetAllCompanies.Query
                {
                    CurrentUserId = TokenService.ReadUserIdFromToken(context),
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
                var result = await sender.Send(query);
                return Results.Ok(result.Value);
            })
            .RequireAuthorization("base-policy")
            .WithTags("Company")
            .Produces<PaginatedResponse<CompanyResponse>>();


    }
}