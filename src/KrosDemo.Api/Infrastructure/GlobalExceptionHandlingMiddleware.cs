namespace KrosDemo.Api.Infrastructure;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private const string ProblemJsonContentType = "application/problem+json";

    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly ApiProblemDetailsFactory _factory;

    public GlobalExceptionHandlingMiddleware(
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        ApiProblemDetailsFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            var problem = _factory.For(ex, context);
            context.Response.StatusCode = problem.Status!.Value;
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJsonContentType);
        }
    }
}