using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Exceptions;
using KrosDemo.Application.Interfaces;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Infrastructure.Data;

public class InvoiceItemRepository : IInvoiceItemRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceItemRepository(ApplicationDbContext context)
    {
        _context = context;
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
            await ThrowMappedConcurrencyAsync(ex, item.Id, cancellationToken);
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
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

            if (databaseValues is null)
                return;

            var current = (InvoiceItem)databaseValues.ToObject();
            throw new ConcurrencyConflictException(nameof(InvoiceItem), id, current);
        }
    }

    private static async Task ThrowMappedConcurrencyAsync(
        DbUpdateConcurrencyException ex,
        int id,
        CancellationToken cancellationToken)
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

        if (databaseValues is null)
            throw new KeyNotFoundException($"InvoiceItem {id} was deleted by another user.");

        var current = (InvoiceItem)databaseValues.ToObject();
        throw new ConcurrencyConflictException(nameof(InvoiceItem), id, current);
    }
}