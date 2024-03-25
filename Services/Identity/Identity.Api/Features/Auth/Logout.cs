using Carter;
using FluentValidation;
using Identity.Api.Contracts;
using Identity.Api.Data;
using Identity.Api.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.Auth;

public static class Logout
{
    public sealed record Command(string RefreshToken) : IRequest<Result<Unit>>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.RefreshToken).NotEmpty();
        }
    }

    internal sealed class Handler(
        AppDbContext dbContext,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Unit>(new Errors.Validation(
                    nameof(Refresh), validationResult.ToString()));
            }
                
            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens
                    .Any(rt => rt.Token == command.RefreshToken), ct);

            if (user is null)
                return Result.Failure<Unit>(new Errors.Credentials(nameof(Refresh)));

            var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == command.RefreshToken);
            user.RefreshTokens.Remove(oldRefreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);
            return await Unit.Task;
        }
    }
}

public class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/logout", async (RefreshRequest request, ISender sender) =>
        {
            var result = await sender.Send(new Logout.Command(request.RefreshToken));
            
            return result.IsFailure switch
            {
                true when result.Error is Errors.Validation => Results.BadRequest(result.Error),
                true when result.Error is Errors.Credentials => Results.Unauthorized(),
                _ => Results.NoContent()
            };
        })
        .WithTags("Authentication")
        .Produces<LoginResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}