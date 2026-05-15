using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Repositories;

public class InvoiceItemRepository : IInvoiceItemRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InvoiceItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceItems.ToListAsync(cancellationToken);
    }

    public async Task<InvoiceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceItems.FindAsync([id], cancellationToken);
    }

    public async Task UpdateAsync(InvoiceItem item, byte[] originalRowVersion, CancellationToken cancellationToken = default)
    {
        _context.Entry(item).Property(i => i.RowVersion).OriginalValue = originalRowVersion;
        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await ConcurrencyConflictMapper.ThrowOnUpdateConflictAsync<InvoiceItem>(ex, item.Id, cancellationToken);
        }
    }

    public async Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default)
    {
        var item = await _context.InvoiceItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"InvoiceItem {id} not found.");

        _context.Entry(item).Property(i => i.RowVersion).OriginalValue = originalRowVersion;
        _context.InvoiceItems.Remove(item);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await ConcurrencyConflictMapper.HandleDeleteConflictAsync<InvoiceItem>(ex, id, cancellationToken);
        }
    }
}