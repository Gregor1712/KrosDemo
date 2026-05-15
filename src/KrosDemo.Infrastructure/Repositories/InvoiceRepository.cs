using KrosDemo.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Repositories;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<DatabaseResult<IReadOnlyList<Invoice>>> GetPagedAsync(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .AsQueryable();

        query = filter.Apply(query);
        var count = await query.CountAsync(cancellationToken);

        query = sort.Apply(query);
        query = pagination.ApplyOrderById(query);

        var data = await query.ToListAsync(cancellationToken);
        return new DatabaseResult<IReadOnlyList<Invoice>>(data, count);
    }

    public async Task UpdateAsync(Invoice invoice, byte[] originalRowVersion, CancellationToken cancellationToken = default)
    {
        _context.Entry(invoice).Property(i => i.RowVersion).OriginalValue = originalRowVersion;
        _context.Entry(invoice).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await ConcurrencyConflictMapper.ThrowOnUpdateConflictAsync<Invoice>(ex, invoice.Id, cancellationToken);
        }
    }

    public async Task DeleteAsync(int id, byte[] originalRowVersion, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        _context.Entry(invoice).Property(i => i.RowVersion).OriginalValue = originalRowVersion;
        _context.Invoices.Remove(invoice);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await ConcurrencyConflictMapper.HandleDeleteConflictAsync<Invoice>(ex, id, cancellationToken);
        }
    }
}