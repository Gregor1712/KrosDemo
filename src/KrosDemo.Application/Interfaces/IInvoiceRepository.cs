using KrosDemo.Application.Filters;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to a tracked Invoice with optimistic concurrency check
    /// against the supplied original RowVersion. Throws
    /// <see cref="Exceptions.ConcurrencyConflictException"/> on stale RowVersion.
    /// </summary>
    Task UpdateAsync(Invoice invoice, byte[] originalRowVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the Invoice with the given id, checking optimistic concurrency
    /// against the supplied original RowVersion. Idempotent: if the row was
    /// already deleted by someone else, completes without throwing. Throws
    /// <see cref="KeyNotFoundException"/> if the invoice doesn't exist at all,
    /// or <see cref="Exceptions.ConcurrencyConflictException"/> on stale RowVersion.
    /// </summary>
    Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default);
}