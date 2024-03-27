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

public class Refresh : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/refresh", async (RefreshRequest request, ISender sender) =>
            {
                var command = new Command(request.RefreshToken);
                return await sender.Send(command);
            })
            .WithTags(nameof(User))
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
    
    public sealed record Command(
        string RefreshToken
    ) : IRequest<Results<Ok<LoginResponse>, ValidationProblem, UnauthorizedHttpResult>>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.RefreshToken).NotEmpty();
        }
    }
    
    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator,
        ITokenService tokenService
        ) : IRequestHandler<Command, Results<Ok<LoginResponse>, ValidationProblem, UnauthorizedHttpResult>>
    {
        public async Task<Results<Ok<LoginResponse>, ValidationProblem, UnauthorizedHttpResult>> 
            Handle(Command commnad, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(commnad, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens
                    .Any(rt => rt.Token == commnad.RefreshToken), ct);

            if (user is null)
                return TypedResults.Unauthorized();

            var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == commnad.RefreshToken);
            if (oldRefreshToken.Expires < DateTime.UtcNow)
                return TypedResults.Unauthorized();
            
            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
        
            user.RefreshTokens.Remove(oldRefreshToken);
            user.RefreshTokens.Add(refreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);

            return TypedResults.Ok(new LoginResponse(accessToken, refreshToken.Token, refreshToken.Expires));
        }
    }
}