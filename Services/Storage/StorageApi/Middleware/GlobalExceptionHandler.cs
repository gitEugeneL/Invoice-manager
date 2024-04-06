using Microsoft.AspNetCore.Diagnostics;

namespace StorageApi.Middleware;

public class GlobalExceptionHandler: IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync("Something went wrong", cancellationToken);
        return true;
    }
}
