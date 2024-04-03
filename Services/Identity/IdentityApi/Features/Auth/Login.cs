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
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;

namespace IdentityApi.Features.Auth;

public class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/login", async (LoginRequest request, ISender sender) =>
            {
                var command = new Command(
                    Email: request.Email,
                    Password: request.Password
                );
                return await sender.Send(command);
            })
            .WithTags(nameof(User))
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }
    
    public sealed record Command(
        string Email, 
        string Password
        ) : IRequest<Results<Ok<LoginResponse>, NotFound<string>, ValidationProblem>>; 
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(c => c.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }

    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator,
        IPasswordService passwordService,
        ITokenService tokenService,
        IConfiguration configuration
        ) : IRequestHandler<Command, Results<Ok<LoginResponse>, NotFound<string>, ValidationProblem>>
    {
        public async Task<Results<Ok<LoginResponse>, NotFound<string>, ValidationProblem>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email.ToLower().Equals(command.Email.ToLower()), ct);

            if (user is null || !passwordService.VerifyPasswordHash(command.Password, user.PwdHash, user.PwdSalt))
                return TypedResults.NotFound("login or password is incorrect");
            
            var refreshTokenMaxCount = int.Parse(configuration
                .GetSection("Authentication:RefreshTokenMaxCount").Value!);

            if (user.RefreshTokens.Count >= refreshTokenMaxCount)
                user.RefreshTokens.Remove(user.RefreshTokens.OrderBy(rt => rt.Expires).First());
            
            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
            
            user.RefreshTokens.Add(refreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Ok(
                new LoginResponse(accessToken, refreshToken.Token, refreshToken.Expires));
        }
    }
}
