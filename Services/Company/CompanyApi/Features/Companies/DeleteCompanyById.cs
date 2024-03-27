using Carter;
using CompanyApi.Data;
using CompanyApi.Domain;
using CompanyApi.Helpers;
using CompanyApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public class DeleteCompanyById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/v1/company/{id:guid}", async (Guid id, HttpContext context, ISender sender) =>
            {
                var command = new Command(TokenService.ReadUserIdFromToken(context), id);
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Company))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record Command(
        Guid CurrentUserId,
        Guid CompanyId
    ) : IRequest<Results<NoContent, NotFound<string>>>;

    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Command, Results<NoContent, NotFound<string>>>
    {
        public async Task<Results<NoContent, NotFound<string>>> Handle(Command command, CancellationToken ct)
        {
            var company = await dbContext
                .Companies
                .Where(c => c.Id == command.CompanyId && c.CreatedByUserId == command.CurrentUserId)
                .SingleOrDefaultAsync(ct);

            if (company is null)
                return TypedResults.NotFound($"Company: {command.CompanyId} not found");

            dbContext.Remove(company);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        }
    }
}