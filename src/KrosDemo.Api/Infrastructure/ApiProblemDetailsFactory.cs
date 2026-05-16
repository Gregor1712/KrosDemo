using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Exceptions;

namespace KrosDemo.Api.Infrastructure;

public sealed class ApiProblemDetailsFactory
{
    // SQL Server error numbers for unique-constraint violations.
    private const int SqlUniqueIndexViolation = 2601;
    private const int SqlUniqueConstraintViolation = 2627;

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
            case DbUpdateException dbUpdate when TryGetUniqueViolation(dbUpdate, out var sql):
                return Build(httpContext, StatusCodes.Status409Conflict, "Duplicate value", sql!.Message);
            default:
                return BuildUnexpected(httpContext, exception);
        }
    }

    private static bool TryGetUniqueViolation(DbUpdateException ex, out SqlException? sqlException)
    {
        if (ex.InnerException is SqlException sql &&
            (sql.Number == SqlUniqueIndexViolation || sql.Number == SqlUniqueConstraintViolation))
        {
            sqlException = sql;
            return true;
        }

        sqlException = null;
        return false;
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