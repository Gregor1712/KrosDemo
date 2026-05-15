using KrosDemo.Domain.Events;
using Microsoft.Extensions.Logging;

namespace KrosDemo.Application.Outbox.Handlers;

public sealed class InvoiceSentEmailHandler : IOutboxMessageHandler<InvoiceSentEvent>
{
    private readonly ILogger<InvoiceSentEmailHandler> _logger;

    public InvoiceSentEmailHandler(ILogger<InvoiceSentEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(InvoiceSentEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[email+isdoc] Would deliver invoice {InvoiceNumber} to {CustomerName} (IČO {BusinessId}); ISDOC export would be generated.",
            domainEvent.InvoiceNumber,
            domainEvent.CustomerName,
            domainEvent.CustomerBusinessId ?? "n/a");
        return Task.CompletedTask;
    }
}