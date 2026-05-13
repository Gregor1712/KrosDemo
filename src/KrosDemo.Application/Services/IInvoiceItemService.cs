using KrosDemo.Application.DTOs;

namespace KrosDemo.Application.Services;

public interface IInvoiceItemService
{
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