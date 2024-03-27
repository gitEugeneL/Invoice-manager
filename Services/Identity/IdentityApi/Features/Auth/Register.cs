using Carter;
using Carter.ModelBinding;
using FluentValidation;
using IdentityApi.Contracts;
using IdentityApi.Data;
using IdentityApi.Domain.Entities;
using IdentityApi.Security.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Features.Auth;

public class Register : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (RegisterRequest request, ISender sender) =>
            {
                var command = new Command(
                    Email: request.Email,
                    Password: request.Password
                );
                return await sender.Send(command);
            })
            .WithTags(nameof(User))
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);
    }
    
    public sealed record Command(
        string Email, 
        string Password
        ) : IRequest<Results<Created<UserResponse>, Conflict<string>, ValidationProblem>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(u => u.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(u => u.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20)
                .Matches(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$")
                .WithMessage("The password must contain at least one letter, one special character, and one digit");
        }
    }
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator,
        IPasswordService passwordService
    ) : IRequestHandler<Command, Results<Created<UserResponse>, Conflict<string>, ValidationProblem>>
    {
        public async Task<Results<Created<UserResponse>, Conflict<string>, ValidationProblem>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());

            if (await dbContext.Users.AnyAsync(u => u.Email.ToLower().Equals(command.Email.ToLower()), ct))
                return TypedResults.Conflict($"User: {command.Email} already exists");
            
            passwordService.CreatePasswordHash(command.Password, out var hash, out var salt);
            var user = new User
            {
                Email = command.Email,
                PwdHash = hash,
                PwdSalt = salt
            };
            await dbContext.Users.AddAsync(user, ct);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Created(user.Id.ToString(), new UserResponse(user));
        }
    }
}