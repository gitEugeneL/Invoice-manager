using System.Security.Claims;

namespace InvoiceApi.Utils;

public static class BaseService
{
    public static Guid ReadUserIdFromToken(HttpContext httpContext)
    {
        return Guid.Parse(
            httpContext
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}