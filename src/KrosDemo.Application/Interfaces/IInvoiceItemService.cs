using KrosDemo.Application.DTOs;

namespace KrosDemo.Application.Interfaces;

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