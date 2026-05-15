using KrosDemo.Application.DTOs;
using KrosDemo.Application.Filters;
using KrosDemo.Application.RequestHelpers;

namespace KrosDemo.Application.Services;

public interface IInvoiceService
{
    Task<InvoiceDTO> GetInvoiceByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResponse<List<InvoiceDTO>>> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default);

    Task<InvoiceDTO> CreateInvoiceAsync(
        InvoiceCreateDTO dto,
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