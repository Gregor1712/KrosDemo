using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using KrosDemo.Api.Infrastructure;
using KrosDemo.Application.Exceptions;

namespace KrosDemo.Tests.Infrastructure;

public class ApiProblemDetailsFactoryTests
{
    [Fact]
    public void For_KeyNotFound_ReturnsProblem404()
    {
        var factory = new ApiProblemDetailsFactory(new StubEnv("Development"));
        var problem = factory.For(new KeyNotFoundException("Invoice 5 not found."), Ctx("/api/invoices/5"));

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Resource not found", problem.Title);
        Assert.Equal("Invoice 5 not found.", problem.Detail);
        Assert.Equal("/api/invoices/5", problem.Instance);
    }

    [Fact]
    public void For_PreconditionRequired_ReturnsProblem428()
    {
        var factory = new ApiProblemDetailsFactory(new StubEnv("Development"));
        var problem = factory.For(PreconditionRequiredException.MissingIfMatch(), Ctx("/api/invoices/1"));

        Assert.Equal(StatusCodes.Status428PreconditionRequired, problem.Status);
        Assert.Equal("Precondition required", problem.Title);
        Assert.Contains("If-Match", problem.Detail!);
    }

    [Fact]
    public void For_ConcurrencyConflict_ReturnsProblem409WithExtensions()
    {
        var current = new { invoiceNumber = "INV-001" };
        var ex = new ConcurrencyConflictException("Invoice", 42, current);

        var factory = new ApiProblemDetailsFactory(new StubEnv("Development"));
        var problem = factory.For(ex, Ctx("/api/invoices/42"));

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal("Concurrency conflict", problem.Title);
        Assert.Equal("Invoice", problem.Extensions["entityName"]);
        Assert.Equal(42, problem.Extensions["entityId"]);
        Assert.Same(current, problem.Extensions["currentDatabaseValues"]);
    }

    [Fact]
    public void For_GenericException_InDevelopment_IncludesStackTraceAndType()
    {
        var ex = ThrownException(new InvalidOperationException("kaboom"));
        var factory = new ApiProblemDetailsFactory(new StubEnv("Development"));

        var problem = factory.For(ex, Ctx("/api/x"));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("kaboom", problem.Detail);
        Assert.Equal(typeof(InvalidOperationException).FullName, problem.Extensions["exceptionType"]);
        Assert.NotNull(problem.Extensions["stackTrace"]);
    }

    [Fact]
    public void For_GenericException_InProduction_HidesInternals()
    {
        var ex = ThrownException(new InvalidOperationException("kaboom"));
        var factory = new ApiProblemDetailsFactory(new StubEnv("Production"));

        var problem = factory.For(ex, Ctx("/api/x"));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.DoesNotContain("kaboom", problem.Detail!);
        Assert.False(problem.Extensions.ContainsKey("exceptionType"));
        Assert.False(problem.Extensions.ContainsKey("stackTrace"));
    }

    private static HttpContext Ctx(string path)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        return ctx;
    }

    private static Exception ThrownException(Exception ex)
    {
        try { throw ex; }
        catch (Exception caught) { return caught; }
    }

    private sealed class StubEnv : IHostEnvironment
    {
        public StubEnv(string envName) => EnvironmentName = envName;

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "KrosDemo.Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}