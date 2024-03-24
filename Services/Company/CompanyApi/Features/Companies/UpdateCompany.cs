using Carter;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CompanyApi.Features.Companies;

public static class UpdateCompany
{
    public sealed class Command : IRequest<Result<CompanyResponse>>
    {
        public Guid CurrentUserId { get; init; }
        
        public Guid CompanyId { get; init; }
        
        public string? Name { get; init; }
        
        public string? TaxNumber { get; init; }
        
        public string? City { get; init; }
        
        public string? Street { get; init; }
        
        public string? HouseNumber { get; init; }
        
        public string? PostalCode { get; init; }
    }
    
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
    ) : IRequestHandler<Command, Result<CompanyResponse>>
    {
        public async Task<Result<CompanyResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<CompanyResponse>(new Errors.Validation(
                    nameof(UpdateCompany), validationResult.ToString()));
            
            var company = await dbContext
                .Companies
                .Where(c => c.Id == request.CompanyId && c.CreatedByUserId == request.CurrentUserId)
                .SingleOrDefaultAsync(cancellationToken);

            if (company is null)
                return Result.Failure<CompanyResponse>(new Errors.NotFound(
                    nameof(UpdateCompany), request.CompanyId));
            
            company.Name = request.Name ?? company.Name;
            company.Name = request.Name ?? company.Name;
            company.TaxNumber = request.TaxNumber ?? company.TaxNumber;
            company.City = request.City ?? company.City;
            company.Street = request.Street ?? company.Street;
            company.HouseNumber = request.HouseNumber ?? company.HouseNumber;
            company.PostalCode = request.PostalCode ?? company.PostalCode;

            dbContext.Companies.Update(company);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            return new CompanyResponse(company);
        }
    }
}

public class UpdateCompanyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/company", async (HttpContext context, UpdateCompanyRequest request, ISender sender) =>
            {
                var result = await sender.Send(new UpdateCompany.Command
                {
                    CurrentUserId = TokenService.ReadUserIdFromToken(context),
                    CompanyId = request.CompanyId,
                    Name = request.Name,
                    TaxNumber = request.TaxNumber,
                    City = request.City,
                    Street = request.Street,
                    HouseNumber = request.HouseNumber,
                    PostalCode = request.PostalCode
                });
                
                return result.IsFailure switch
                {
                    true when result.Error is Errors.Validation => Results.BadRequest(result.Error),
                    true when result.Error is Errors.NotFound => Results.NotFound(result.Error), 
                    _ => Results.Ok(result.Value)
                };
            })
            .RequireAuthorization("base-policy")
            .Produces<CompanyResponse>()
            .WithTags("Company")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
}