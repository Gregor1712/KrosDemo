using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Exceptions;

namespace KrosDemo.Api.Infrastructure;

public class ConcurrencyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyConflictException ccx)
            return false;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Concurrency conflict",
            Detail = ccx.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["entityName"] = ccx.EntityName;
        problem.Extensions["entityId"] = ccx.EntityId;
        problem.Extensions["currentDatabaseValues"] = ccx.CurrentDatabaseValues;

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}