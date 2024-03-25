using Carter;
using FluentValidation;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Security.Interfaces;
using Identity.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;

namespace Identity.Api.Features.Auth;

public static class Login
{
    public sealed record Command(
        string Email, 
        string Password
        ) : IRequest<Result<LoginResponse>>; 
    
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
        ) : IRequestHandler<Command, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<LoginResponse>(new Errors.Validation(
                    nameof(Login), validationResult.ToString()));
            }

            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email.ToLower().Equals(command.Email.ToLower()), ct);

            if (user is null || !passwordService.VerifyPasswordHash(command.Password, user.PwdHash, user.PwdSalt))
            {
                return Result.Failure<LoginResponse>(new Errors.NotFound(
                    nameof(Login), command.Email));
            }
                
            var refreshTokenMaxCount = int.Parse(configuration
                .GetSection("Authentication:RefreshTokenMaxCount").Value!);

            if (user.RefreshTokens.Count >= refreshTokenMaxCount)
            {
                user.RefreshTokens
                    .Remove(user.RefreshTokens.OrderBy(rt => rt.Expires).First());
            }
            
            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
            
            user.RefreshTokens.Add(refreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);

            return new LoginResponse(accessToken, refreshToken.Token, refreshToken.Expires, accessToken);
        }
    }
}

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/login", async (LoginRequest request, ISender sender) =>
            {
                var result = await sender.Send(new Login.Command(
                    request.Email,
                    request.Password)
                );
                
                return result.IsFailure switch
                {
                    true when result.Error is Errors.Validation => Results.BadRequest(result.Error),
                    true when result.Error is Errors.NotFound => Results.NotFound(result.Error),
                    _ => Results.Ok(result.Value)
                };
            })
            .WithTags("Authentication")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}