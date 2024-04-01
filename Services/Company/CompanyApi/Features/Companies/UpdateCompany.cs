using Carter;
using Carter.ModelBinding;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Domain.Entities;
using CompanyApi.Helpers;
using CompanyApi.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public class UpdateCompany : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/company", async (HttpContext context, UpdateCompanyRequest request, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
                    CompanyId: request.CompanyId,
                    Name: request.Name,
                    TaxNumber: request.TaxNumber,
                    City: request.City,
                    Street: request.Street,
                    HouseNumber: request.HouseNumber,
                    PostalCode: request.PostalCode
                );
                return await sender.Send(command);
            })
            .RequireAuthorization(AppConstants.BaseAuthPolicy)
            .WithTags(nameof(Company))
            .Produces<CompanyResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public sealed record Command(
        Guid CurrentUserId,
        Guid CompanyId,
        string? Name,
        string? TaxNumber,
        string? City,
        string? Street,
        string? HouseNumber,
        string? PostalCode
    ) : IRequest<Results<Ok<CompanyResponse>, NotFound<string>, ValidationProblem>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CompanyId)
                .NotEmpty();
        
            RuleFor(c => c.Name)
                .MaximumLength(100);

            RuleFor(c => c.TaxNumber)
                .Length(10)
                .Matches(@"^\d+$");

            RuleFor(c => c.City)
                .MaximumLength(50);

            RuleFor(c => c.Street)
                .MaximumLength(50);

            RuleFor(c => c.HouseNumber)
                .MaximumLength(10);

            RuleFor(c => c.PostalCode)
                .MaximumLength(10);
        }
    }

    internal sealed class Handler(
        AppDbContext dbContext, 
        IValidator<Command> validator
    ) : IRequestHandler<Command, Results<Ok<CompanyResponse>, NotFound<string>, ValidationProblem>>
    {
        public async Task<Results<Ok<CompanyResponse>, NotFound<string>, ValidationProblem>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var company = await dbContext
                .Companies
                .Where(c => c.Id == command.CompanyId && c.CreatedByUserId == command.CurrentUserId)
                .SingleOrDefaultAsync(ct);

            if (company is null)
                return TypedResults.NotFound($"Company {command.CompanyId} not found");
            
            company.Name = command.Name ?? company.Name;
            company.Name = command.Name ?? company.Name;
            company.TaxNumber = command.TaxNumber ?? company.TaxNumber;
            company.City = command.City ?? company.City;
            company.Street = command.Street ?? company.Street;
            company.HouseNumber = command.HouseNumber ?? company.HouseNumber;
            company.PostalCode = command.PostalCode ?? company.PostalCode;

            dbContext.Companies.Update(company);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Ok(new CompanyResponse(company));
        }
    }
}