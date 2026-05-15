namespace KrosDemo.Domain.Events;

public sealed record InvoiceSentEvent(
    string InvoiceNumber,
    string CustomerName,
    string? CustomerBusinessId,
    DateTime OccurredOnUtc) : IDomainEvent;