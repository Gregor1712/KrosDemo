using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Application.Interfaces;

public interface IServerService
{
    Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);
}