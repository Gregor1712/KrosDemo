using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Interfaces;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Api.Infrastructure;

public class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;

    public DatabaseInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync(cancellationToken);

        var csvSeeder = scope.ServiceProvider.GetRequiredService<ICsvDataSeeder>();
        await csvSeeder.SeedDataAsync();

        var userSeeder = scope.ServiceProvider.GetRequiredService<IUserDataSeeder>();
        await userSeeder.SeedUsers();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}