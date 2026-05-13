using KrosDemo.Application.Filters;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceService
{
    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination);
}