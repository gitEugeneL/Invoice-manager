using System.Security.Claims;

namespace InvoiceApi.Services;

public static class TokenService
{
    public static Guid ReadUserIdFromToken(HttpContext httpContext)
    {
        return Guid.Parse(
            httpContext
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}