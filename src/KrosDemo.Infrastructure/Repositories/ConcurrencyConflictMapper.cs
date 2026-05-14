using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Exceptions;

namespace KrosDemo.Infrastructure.Repositories;

internal static class ConcurrencyConflictMapper
{
    public static async Task ThrowOnUpdateConflictAsync<TEntity>(
        DbUpdateConcurrencyException ex,
        object id,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

        if (databaseValues is null)
            throw new KeyNotFoundException($"{typeof(TEntity).Name} {id} was deleted by another user.");

        var current = (TEntity)databaseValues.ToObject();
        throw new ConcurrencyConflictException(typeof(TEntity).Name, id, current);
    }

    public static async Task HandleDeleteConflictAsync<TEntity>(
        DbUpdateConcurrencyException ex,
        object id,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

        // Row already gone — DELETE is idempotent per RFC 9110, treat as success.
        if (databaseValues is null)
            return;

        var current = (TEntity)databaseValues.ToObject();
        throw new ConcurrencyConflictException(typeof(TEntity).Name, id, current);
    }
}