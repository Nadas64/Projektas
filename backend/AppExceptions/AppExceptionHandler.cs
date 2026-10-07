using Microsoft.AspNetCore.Diagnostics;

namespace backend.Exceptions;

public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not AppException appException)
        {
            return false;
        }

        context.Response.StatusCode = appException.StatusCode;
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(appException.Message, cancellationToken);
        return true;
    }
}