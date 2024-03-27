using Carter;
using Carter.ModelBinding;
using FluentValidation;
using IdentityApi.Contracts;
using IdentityApi.Data;
using IdentityApi.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Features.Auth;

public class Logout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/logout", async (RefreshRequest request, ISender sender) =>
            {
                var command = new Command(request.RefreshToken);
                return await sender.Send(command);
            })
            .WithTags(nameof(User))
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
    
    public sealed record Command(
        string RefreshToken
    ) : IRequest<Results<NoContent, ValidationProblem, UnauthorizedHttpResult>>;

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
    ) : IRequestHandler<Command, Results<NoContent, ValidationProblem, UnauthorizedHttpResult>>
    {
        public async Task<Results<NoContent, ValidationProblem, UnauthorizedHttpResult>> 
            Handle(Command command, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
                return TypedResults.ValidationProblem(validationResult.GetValidationProblems());
            
            var user = await dbContext
                .Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens
                    .Any(rt => rt.Token == command.RefreshToken), ct);

            if (user is null)
                return TypedResults.Unauthorized();
            
            var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == command.RefreshToken);
            user.RefreshTokens.Remove(oldRefreshToken);

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync(ct);
            
            return TypedResults.NoContent();
        }
    }
}
