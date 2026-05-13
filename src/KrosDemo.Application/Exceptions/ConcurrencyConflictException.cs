namespace KrosDemo.Application.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public string EntityName { get; }
    public object EntityId { get; }
    public object? CurrentDatabaseValues { get; }

    public ConcurrencyConflictException(string entityName, object entityId, object? currentDatabaseValues)
        : base($"{entityName} with id {entityId} was modified by another user. Reload the entity and try again.")
    {
        EntityName = entityName;
        EntityId = entityId;
        CurrentDatabaseValues = currentDatabaseValues;
    }
}