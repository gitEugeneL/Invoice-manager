using Carter;
using FluentValidation;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Security.Interfaces;
using Identity.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.Auth;

public static class Refresh
{
    public sealed record Command(string RefreshToken) : IRequest<Result<LoginResponse>>;
    
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
        ) : IRequestHandler<Command, Result<LoginResponse>>
    {
        public async Task<Result<LoginResponse>> Handle(Command commnad, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(commnad, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<LoginResponse>(new Errors.Validation(
                    nameof(Refresh), validationResult.ToString()));
            }
                
            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens
                    .Any(rt => rt.Token == commnad.RefreshToken), ct);

            if (user is null)
                return Result.Failure<LoginResponse>(new Errors.Credentials(nameof(Refresh)));

            var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == commnad.RefreshToken);
            if (oldRefreshToken.Expires < DateTime.UtcNow)
                return Result.Failure<LoginResponse>(new Errors.Credentials(nameof(Refresh)));
            
            var accessToken = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken(user);
        
            user.RefreshTokens.Remove(oldRefreshToken);
            user.RefreshTokens.Add(refreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);
            
            return new LoginResponse(accessToken, refreshToken.Token, refreshToken.Expires);
        }
    }
}

public class RefreshEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/refresh", async (RefreshRequest request, ISender sender) =>
            {
                var result = await sender.Send(new Refresh.Command(request.RefreshToken));

                return result.IsFailure switch
                {
                    true when result.Error is Errors.Validation => Results.BadRequest(result.Error),
                    true when result.Error is Errors.Credentials => Results.Unauthorized(),
                    _ => Results.Ok(result.Value)
                };

            })
            .WithTags("Authentication")
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}