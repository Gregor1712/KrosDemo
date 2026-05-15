using KrosDemo.Domain.Events;
using Microsoft.Extensions.Logging;

namespace KrosDemo.Application.Outbox.Handlers;

public sealed class InvoiceIssuedAuditHandler : IOutboxMessageHandler<InvoiceIssuedEvent>
{
    private readonly ILogger<InvoiceIssuedAuditHandler> _logger;

    public InvoiceIssuedAuditHandler(ILogger<InvoiceIssuedAuditHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(InvoiceIssuedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[audit] Invoice {InvoiceNumber} issued for {CustomerName} at {OccurredOnUtc:O}.",
            domainEvent.InvoiceNumber,
            domainEvent.CustomerName,
            domainEvent.OccurredOnUtc);
        return Task.CompletedTask;
    }
}