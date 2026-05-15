namespace KrosDemo.Domain.Events;

public sealed record InvoiceIssuedEvent(
    string InvoiceNumber,
    string CustomerName,
    string? CustomerBusinessId,
    DateTime OccurredOnUtc) : IDomainEvent;