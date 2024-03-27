namespace IdentityApi.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpires,
    string AccessTokenType = "Bearer");