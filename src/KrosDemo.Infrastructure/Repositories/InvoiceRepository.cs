using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.Exceptions;
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

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .AsQueryable();

        //query = ApplyFilters(query, filter);
        query = filter.Apply(query);
        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sort);

        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var items = await query
            .Skip(skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task UpdateAsync(Invoice invoice, byte[] originalRowVersion, CancellationToken cancellationToken = default)
    {
        _context.Entry(invoice).Property(i => i.RowVersion).OriginalValue = originalRowVersion;

        // PUT is a full replace per HTTP semantics — force UPDATE even when no
        // field actually changed, so the RowVersion check in the WHERE clause
        // still runs against a stale If-Match.
        _context.Entry(invoice).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await ThrowMappedConcurrencyAsync(ex, invoice.Id, cancellationToken);
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
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

            // Row already gone — DELETE is idempotent per RFC 9110, treat as success.
            if (databaseValues is null)
                return;

            var current = (Invoice)databaseValues.ToObject();
            throw new ConcurrencyConflictException(nameof(Invoice), id, current);
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
            throw new KeyNotFoundException($"Invoice {id} was deleted by another user.");

        var current = (Invoice)databaseValues.ToObject();
        throw new ConcurrencyConflictException(nameof(Invoice), id, current);
    }

    // private static IQueryable<Invoice> ApplyFilters(IQueryable<Invoice> query, InvoiceFilter filter)
    // {
    //     if (!string.IsNullOrWhiteSpace(filter.InvoiceNumber))
    //         query = query.Where(i => i.InvoiceNumber.Contains(filter.InvoiceNumber));
    //
    //     if (!string.IsNullOrWhiteSpace(filter.CustomerName))
    //         query = query.Where(i => i.CustomerName.Contains(filter.CustomerName));
    //
    //     if (!string.IsNullOrWhiteSpace(filter.CustomerBusinessId))
    //         query = query.Where(i => i.CustomerBusinessId == filter.CustomerBusinessId);
    //
    //     if (filter.Status.HasValue)
    //         query = query.Where(i => i.Status == filter.Status.Value);
    //
    //     if (filter.IssueDateFrom.HasValue)
    //         query = query.Where(i => i.IssueDate >= filter.IssueDateFrom.Value);
    //
    //     if (filter.IssueDateTo.HasValue)
    //         query = query.Where(i => i.IssueDate <= filter.IssueDateTo.Value);
    //
    //     if (filter.DueDateFrom.HasValue)
    //         query = query.Where(i => i.DueDate >= filter.DueDateFrom.Value);
    //
    //     if (filter.DueDateTo.HasValue)
    //         query = query.Where(i => i.DueDate <= filter.DueDateTo.Value);
    //
    //     return query;
    // }

    private static IQueryable<Invoice> ApplySort(IQueryable<Invoice> query, SortFilter sort)
    {
        var descending = sort.Direction == SortDirection.Desc;

        return sort.SortBy?.ToLowerInvariant() switch
        {
            "invoicenumber" => descending ? query.OrderByDescending(i => i.InvoiceNumber) : query.OrderBy(i => i.InvoiceNumber),
            "customername"  => descending ? query.OrderByDescending(i => i.CustomerName)  : query.OrderBy(i => i.CustomerName),
            "issuedate"     => descending ? query.OrderByDescending(i => i.IssueDate)     : query.OrderBy(i => i.IssueDate),
            "duedate"       => descending ? query.OrderByDescending(i => i.DueDate)       : query.OrderBy(i => i.DueDate),
            "status"        => descending ? query.OrderByDescending(i => i.Status)        : query.OrderBy(i => i.Status),
            _               => query.OrderByDescending(i => i.IssueDate),
        };
    }
}