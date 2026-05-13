using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Interfaces;

public interface IInvoiceItemRepository
{
    Task<InvoiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateAsync(InvoiceItem item, byte[] originalRowVersion, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default);
}