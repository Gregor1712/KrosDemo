using Microsoft.AspNetCore.Mvc;
using KrosDemo.Application.Exceptions;

namespace KrosDemo.Api.Infrastructure;

public sealed class ApiProblemDetailsFactory
{
    private readonly IHostEnvironment _env;

    public ApiProblemDetailsFactory(IHostEnvironment env) => _env = env;

    public ProblemDetails For(Exception exception, HttpContext httpContext)
    {
        switch (exception)
        {
            case KeyNotFoundException keyNotFound:
                return Build(httpContext, StatusCodes.Status404NotFound, "Resource not found", keyNotFound.Message);
            case PreconditionRequiredException precondition:
                return Build(httpContext, StatusCodes.Status428PreconditionRequired, "Precondition required", precondition.Message);
            case ConcurrencyConflictException concurrency:
                return BuildConcurrency(httpContext, concurrency);
            default:
                return BuildUnexpected(httpContext, exception);
        }
    }

    private static ProblemDetails Build(HttpContext ctx, int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail,
        Instance = ctx.Request.Path
    };

    private static ProblemDetails BuildConcurrency(HttpContext ctx, ConcurrencyConflictException ccx)
    {
        var problem = Build(ctx, StatusCodes.Status409Conflict, "Concurrency conflict", ccx.Message);
        problem.Extensions["entityName"] = ccx.EntityName;
        problem.Extensions["entityId"] = ccx.EntityId;
        problem.Extensions["currentDatabaseValues"] = ccx.CurrentDatabaseValues;
        return problem;
    }

    private ProblemDetails BuildUnexpected(HttpContext ctx, Exception exception)
    {
        var detail = _env.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";

        var problem = Build(ctx, StatusCodes.Status500InternalServerError, "Unexpected error", detail);

        if (_env.IsDevelopment())
        {
            problem.Extensions["exceptionType"] = exception.GetType().FullName;
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problem;
    }
}