using System.Text.Json;
using KrosDemo.Application.Outbox;
using KrosDemo.Domain.Events;
using KrosDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KrosDemo.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;
    private const int MaxRetries = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started; polling every {Interval}s.", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Outbox processing batch failed; will retry next tick.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (TaskCanceledException) { /* shutdown */ }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            await DispatchAsync(scope.ServiceProvider, message, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchAsync(
        IServiceProvider services,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventType = Type.GetType(message.Type)
                ?? throw new InvalidOperationException($"Unknown outbox event type '{message.Type}'.");

            var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, eventType)
                ?? throw new InvalidOperationException($"Outbox payload deserialized to null for type '{message.Type}'.");

            var handlerInterface = typeof(IOutboxMessageHandler<>).MakeGenericType(eventType);
            var handleMethod = handlerInterface.GetMethod("HandleAsync")!;
            var handlers = services.GetServices(handlerInterface);

            var handlerCount = 0;
            foreach (var handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }
                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
                handlerCount++;
            }

            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = null;
            _logger.LogInformation(
                "Outbox message {MessageId} ({EventType}) dispatched to {HandlerCount} handler(s).",
                message.Id, eventType.Name, handlerCount);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message;
            _logger.LogError(ex,
                "Outbox message {MessageId} dispatch failed (retry {RetryCount}/{MaxRetries}).",
                message.Id, message.RetryCount, MaxRetries);
        }
    }
}