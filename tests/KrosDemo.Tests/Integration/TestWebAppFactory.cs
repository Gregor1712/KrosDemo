using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Tests.Integration;

public class TestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const int SeededInvoiceId = 10;
    public byte[] SeededRowVersion { get; private set; } = [];

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Replace SQL Server with shared in-memory SQLite (one connection,
            // kept open for the factory's lifetime, so the DB persists between
            // requests within a test run).
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(opts => opts
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, SqliteRowVersionModelCustomizer>()
                .AddInterceptors(new BumpRowVersionInterceptor()));

            // Drop the production seeder hosted service — tests seed manually below.
            services.RemoveAll<IHostedService>();

            // Replace JWT auth with a pass-through handler so [Authorize] endpoints
            // are reachable without minting real tokens. Claims cover both roles
            // used by the controllers (User, Admin).
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        // BumpRowVersionInterceptor will overwrite RowVersion on save —
        // capture the post-save value and expose it to tests via SeededRowVersion.
        var invoice = new Invoice
        {
            Id = SeededInvoiceId,
            InvoiceNumber = "INV-TEST",
            CustomerName = "Test Customer",
            CurrencyCode = "EUR",
            IssueDate = new DateTime(2026, 01, 15),
            DueDate = new DateTime(2026, 02, 15),
            Status = InvoiceStatus.Issued
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        SeededRowVersion = invoice.RowVersion;
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}