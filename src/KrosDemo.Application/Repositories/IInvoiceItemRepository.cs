using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Repositories;

public interface IInvoiceItemRepository
{
    Task<List<InvoiceItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateAsync(InvoiceItem item, byte[] originalRowVersion, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default);
}