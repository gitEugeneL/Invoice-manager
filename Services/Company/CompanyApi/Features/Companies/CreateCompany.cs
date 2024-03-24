using Carter;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Entities;
using CompanyApi.Shared;
using FluentValidation;
using MediatR;

namespace CompanyApi.Features.Companies;

public static class CreateCompany
{
    public sealed class Command : IRequest<Result<CompanyResponse>>
    {
        public Guid CurrentUserId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string TaxNumber { get; init; } = string.Empty;

        public string City { get; init; } = string.Empty;

        public string Street { get; init; } = string.Empty;

        public string HouseNumber { get; init; } = string.Empty;

        public string PostalCode { get; init; } = string.Empty;
    }
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(c => c.TaxNumber)
                .NotEmpty()
                .Length(10)
                .Matches(@"^\d+$");

            RuleFor(c => c.City)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(c => c.Street)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(c => c.HouseNumber)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(c => c.PostalCode)
                .NotEmpty()
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
                    nameof(CreateCompany), validationResult.ToString()));
            
            var company = new Company
            {
                CreatedByUserId = request.CurrentUserId,
                Name = request.Name,
                TaxNumber = request.TaxNumber,
                City = request.City,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                PostalCode = request.PostalCode
            };

            await dbContext
                .Companies
                .AddAsync(company, cancellationToken);
            await dbContext
                .SaveChangesAsync(cancellationToken);

            return new CompanyResponse(company);
        }
    }
}

public class CreateCompanyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/company", async (HttpContext context, CreateCompanyRequest request, ISender sender) => 
            { 
                var result = await sender.Send(new CreateCompany.Command 
                {
                    CurrentUserId = TokenService.ReadUserIdFromToken(context),
                    Name = request.Name,
                    TaxNumber = request.TaxNumber,
                    City = request.City,
                    Street = request.Street,
                    HouseNumber = request.HouseNumber,
                    PostalCode = request.PostalCode
                });

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Created(result.Value.CompanyId.ToString(), result.Value); 
            })
            .RequireAuthorization("base-policy")
            .WithTags("Company")
            .Produces<CompanyResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}