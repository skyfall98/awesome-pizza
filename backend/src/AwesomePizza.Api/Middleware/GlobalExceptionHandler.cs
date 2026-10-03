using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AwesomePizza.Api.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService): IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        int? statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            ConflictException => StatusCodes.Status409Conflict,
            _ => null
        };
        if (statusCode is null)
        {
            return false;
        }

        logger.LogWarning("Request failed with status {StatusCode}: {Message}", statusCode, exception.Message);

        ProblemDetails problemDetails = new ProblemDetails { Status = statusCode, Detail = exception.Message };
        ProblemDetailsContext problemDetailsContext = new ProblemDetailsContext()
        {
            ProblemDetails = problemDetails, HttpContext = context, Exception = exception
        };

        context.Response.StatusCode = statusCode.Value;
        return await problemDetailsService.TryWriteAsync(problemDetailsContext);
    }
}
