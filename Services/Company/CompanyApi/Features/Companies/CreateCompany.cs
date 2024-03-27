using Carter;
using Carter.ModelBinding;
using CompanyApi.Contracts.Companies;
using CompanyApi.Data;
using CompanyApi.Domain;
using CompanyApi.Helpers;
using CompanyApi.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CompanyApi.Features.Companies;

public class CreateCompany : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/company", async (HttpContext context, CreateCompanyRequest request, ISender sender) =>
            {
                var command = new Command(
                    CurrentUserId: TokenService.ReadUserIdFromToken(context),
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
            .Produces<CompanyResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public sealed record Command(
        Guid CurrentUserId,
        string Name,
        string TaxNumber,
        string City,
        string Street,
        string HouseNumber,
        string PostalCode
    ) : IRequest<Results<Created<CompanyResponse>, ValidationProblem>>;

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
    ) : IRequestHandler<Command, Results<Created<CompanyResponse>, ValidationProblem>>
    {
        public async Task<Results<Created<CompanyResponse>, ValidationProblem>> Handle(Command command,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var company = new Company
            {
                CreatedByUserId = command.CurrentUserId,
                Name = command.Name,
                TaxNumber = command.TaxNumber,
                City = command.City,
                Street = command.Street,
                HouseNumber = command.HouseNumber,
                PostalCode = command.PostalCode
            };

            await dbContext.Companies.AddAsync(company, ct);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Created(company.Id.ToString(), new CompanyResponse(company));
        }
    }
}