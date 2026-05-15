using KrosDemo.Domain.Events;

namespace KrosDemo.Application.Outbox;

public interface IOutboxMessageHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}