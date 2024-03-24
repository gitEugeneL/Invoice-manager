using Carter;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public static class GetCompanyById
{
    public sealed class Query : IRequest<Result<CompanyResponse>>
    {
        public Guid CurrentUserId { get; init; }
        
        public Guid CompanyId { get; init; }
    }
    
    internal sealed class Handler(AppDbContext dbContext) : IRequestHandler<Query, Result<CompanyResponse>>
    {
        public async Task<Result<CompanyResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var companyResponse = await dbContext
                .Companies
                .AsNoTracking()
                .Where(c => c.Id == request.CompanyId && c.CreatedByUserId == request.CurrentUserId)
                .Select(company => new CompanyResponse(company))
                .SingleOrDefaultAsync(cancellationToken);

            if (companyResponse is null)
                return Result.Failure<CompanyResponse>(new Errors.NotFound(
                    nameof(GetCompanyById), request.CompanyId));
            
            return companyResponse;
        }
    }
}

public class GetCompanyByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/company/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new GetCompanyById.Query
                {
                    CurrentUserId = TokenService.ReadUserIdFromToken(context),
                    CompanyId = id
                };
                var result = await sender.Send(query);

                return result.IsFailure
                    ? Results.NotFound(result.Error)
                    : Results.Ok(result.Value);
            })
            .RequireAuthorization("base-policy")
            .WithTags("Company")
            .Produces<CompanyResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}