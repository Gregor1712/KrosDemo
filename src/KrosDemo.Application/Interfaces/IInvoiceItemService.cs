using KrosDemo.Application.DTOs;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceItemService
{
    Task<InvoiceItem> UpdateInvoiceItemAsync(
        int id,
        InvoiceItemUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);

    Task DeleteInvoiceItemAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default);
}