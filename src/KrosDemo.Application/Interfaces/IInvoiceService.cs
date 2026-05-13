using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceService
{
    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);

    Task<Invoice> UpdateInvoiceAsync(
        int id,
        InvoiceUpdateDTO dto,
        CancellationToken cancellationToken = default);
}