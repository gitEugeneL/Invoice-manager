using Identity.Api.Models.Dto;
using Identity.Api.Models.Entities;
using Identity.Api.Repositories.Interfaces;
using Identity.Api.Security.Interfaces;
using Identity.Api.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var authGroup = builder.MapGroup("api/v1/auth")
            .WithTags("Authentication");

        authGroup.MapPost("register", Register)
            .WithValidator<CreateUserDto>()
            .Produces<UserResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);

        authGroup.MapPost("login", Login)
            .WithValidator<LoginDto>()
            .Produces<LoginResponseDto>()
            .Produces(StatusCodes.Status404NotFound);

        authGroup.MapPost("refresh", Refresh)
            .WithValidator<RefreshDto>()
            .Produces<LoginResponseDto>()
            .Produces(StatusCodes.Status401Unauthorized);

        authGroup.MapPost("logout", Logout)
            .WithValidator<RefreshDto>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Results<Created<UserResponseDto>, Conflict<string>>> Register(
        [FromBody] CreateUserDto dto,
        IPasswordManager pwdManager,
        IUserRepository repository)
    {
        if (await repository.FindUserByEmail(dto.Email) is not null)
            return TypedResults.Conflict($"User: {dto.Email} already exists");
        
        pwdManager.CreatePasswordHash(dto.Password, out var hash, out var salt);
        var user = new User
        {
            Email = dto.Email,
            PwdHash = hash,
            PwdSalt = salt
        };
        await repository.CreateUser(user);
        return TypedResults.Created(user.Id.ToString(), new UserResponseDto(user));
    }
    
    private static async Task<Results<Ok<LoginResponseDto>, NotFound<string>>> Login(
        [FromBody] LoginDto dto,
        IPasswordManager pwdManager,
        ITokenManager tokenManager,
        IConfiguration configuration,
        IUserRepository repository)
    {
        var user = await repository.FindUserByEmail(dto.Email);
        if (user is null || !pwdManager.VerifyPasswordHash(dto.Password, user.PwdHash, user.PwdSalt))
           return TypedResults.NotFound("login or password is invalid");
        
        var refreshTokenMaxCount = int.Parse(configuration
            .GetSection("Authentication:RefreshTokenMaxCount").Value!);
        
        if (user.RefreshTokens.Count >= refreshTokenMaxCount)
        {
            user.RefreshTokens
                .Remove(user.RefreshTokens.OrderBy(rt => rt.Expires).First());
        }
        
        var accessToken = tokenManager.GenerateAccessToken(user);
        var refreshToken = tokenManager.GenerateRefreshToken(user);
        
        user.RefreshTokens.Add(refreshToken);
        await repository.UpdateUser(user);
        
        return TypedResults.Ok(new LoginResponseDto(accessToken, refreshToken.Token, refreshToken.Expires));
    }

    private static async Task<Results<UnauthorizedHttpResult, Ok<LoginResponseDto>>> Refresh(
        [FromBody] RefreshDto dto,
        ITokenManager tokenManager,
        IUserRepository repository)
    {
        var user = await repository.FindUserByRefreshToken(dto.RefreshToken);
        if (user is null)
            return TypedResults.Unauthorized();

        var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == dto.RefreshToken);
        if (oldRefreshToken.Expires < DateTime.UtcNow)
            return TypedResults.Unauthorized();
        
        var accessToken = tokenManager.GenerateAccessToken(user);
        var refreshToken = tokenManager.GenerateRefreshToken(user);
        
        user.RefreshTokens.Remove(oldRefreshToken);
        user.RefreshTokens.Add(refreshToken);
        await repository.UpdateUser(user);

        return TypedResults.Ok(new LoginResponseDto(accessToken, refreshToken.Token, refreshToken.Expires));
    }

    private static async Task<Results<UnauthorizedHttpResult, NoContent>> Logout(
        [FromBody] RefreshDto dto,
        IUserRepository repository)
    {
        var user = await repository.FindUserByRefreshToken(dto.RefreshToken);
        if (user is null)
            return TypedResults.Unauthorized();
        
        var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == dto.RefreshToken);
        user.RefreshTokens.Remove(oldRefreshToken);
        await repository.UpdateUser(user);
        return TypedResults.NoContent();
    }
}