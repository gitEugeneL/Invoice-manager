using Carter;
using FluentValidation;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Entities;
using Identity.Api.Security.Interfaces;
using Identity.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.Auth;

public static class Register
{
    public sealed record Command(
        string Email, 
        string Password
        ) : IRequest<Result<UserResponse>>;
    
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
    ) : IRequestHandler<Command, Result<UserResponse>>
    {
        public async Task<Result<UserResponse>> Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<UserResponse>(
                    new Errors.Validation(nameof(Register), validationResult.ToString()));
            }
                
            if (await dbContext.Users.AnyAsync(u => u.Email.ToLower().Equals(command.Email.ToLower()), ct))
            {
                return Result.Failure<UserResponse>(
                    new Errors.Conflict(nameof(Register), command.Email));
            }
            
            passwordService.CreatePasswordHash(command.Password, out var hash, out var salt);
            var user = new User
            {
                Email = command.Email,
                PwdHash = hash,
                PwdSalt = salt
            };
            await dbContext.Users.AddAsync(user, ct);
            await dbContext.SaveChangesAsync(ct);
            
            return new UserResponse(user);
        }
    }
}

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (RegisterRequest request, ISender sender) =>
            {
                var result = await sender.Send(new Register.Command(request.Email, request.Password));
                
                return result.IsFailure switch
                {
                    true when result.Error is Errors.Validation => Results.BadRequest(result.Error),
                    true when result.Error is Errors.Conflict => Results.Conflict(result.Error),
                    _ => Results.Created(result.Value.UserId.ToString(), result.Value)
                };
            })
            .WithTags("Authentication")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
    }
}