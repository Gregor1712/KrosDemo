using KrosDemo.Application.DTOs;

namespace KrosDemo.Application.Services;

public interface IInvoiceItemService
{
    Task<List<InvoiceItemDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceItemDTO> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<InvoiceItemDTO> UpdateInvoiceItemAsync(
        int id,
        InvoiceItemUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);

    Task DeleteInvoiceItemAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}