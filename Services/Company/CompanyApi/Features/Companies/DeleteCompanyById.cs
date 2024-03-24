using Carter;
using CompanyApi.Data;
using CompanyApi.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public static class DeleteCompanyById
{
    public sealed class Command : IRequest<Result<Unit>>
    {
        public Guid CurrentUserId { get; init; }
        
        public Guid CompanyId { get; init; }
    }
    
    internal sealed class Handler(AppDbContext dbContext) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var company = await dbContext
                .Companies
                .Where(c => c.Id == request.CompanyId && c.CreatedByUserId == request.CurrentUserId)
                .SingleOrDefaultAsync(cancellationToken);

            if (company is null)
                return Result.Failure<Unit>(new Errors.NotFound(
                    nameof(DeleteCompanyById), request.CompanyId));

            dbContext.Remove(company);
            await dbContext.SaveChangesAsync(cancellationToken);
            return await Unit.Task;
        }
    }
}

public class DeleteCompanyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/company/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var query = new DeleteCompanyById.Command
                {
                    CurrentUserId = TokenService.ReadUserIdFromToken(context),
                    CompanyId = id
                };
                var result = await sender.Send(query);

                return result.IsFailure
                    ? Results.NotFound(result.Error)
                    : Results.NoContent();

            })
            .RequireAuthorization("base-policy")
            .WithTags("Company")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}