using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task<DatabaseResult<IReadOnlyList<Invoice>>> GetPagedAsync(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Invoice invoice, byte[] originalRowVersion, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default);
}