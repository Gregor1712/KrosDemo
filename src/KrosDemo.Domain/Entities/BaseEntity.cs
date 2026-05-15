using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;
using KrosDemo.Domain.Events;

namespace KrosDemo.Domain.Entities;

public class BaseEntity
{
    [Ignore]
    public int Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    [Ignore]
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}