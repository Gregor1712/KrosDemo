using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceService
{
    Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);

    Task<InvoiceDTO> UpdateInvoiceAsync(
        int id,
        InvoiceUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);

    Task DeleteInvoiceAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}