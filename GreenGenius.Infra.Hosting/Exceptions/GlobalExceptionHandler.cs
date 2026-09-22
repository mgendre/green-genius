using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GreenGenius.Infra.Hosting.Exceptions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not KeyNotFoundException)
        {
            return false;
        }
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Resource not found"
            }
        });

    }
}