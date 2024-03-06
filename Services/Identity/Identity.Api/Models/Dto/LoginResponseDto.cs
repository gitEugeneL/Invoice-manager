namespace Identity.Api.Models.Dto;

public sealed class LoginResponseDto(string accessToken, string refreshToken, DateTime refreshTokenExpires)
{
    public string AccessTokenType { get; init; } = "Bearer";
    public string AccessToken { get; init; } = accessToken;
    public string RefreshToken { get; init; } = refreshToken;
    public DateTime RefreshTokenExpires { get; init; } = refreshTokenExpires;
}