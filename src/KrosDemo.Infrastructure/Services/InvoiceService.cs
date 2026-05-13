using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Exceptions;
using KrosDemo.Application.Filters;
using KrosDemo.Application.Interfaces;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetInvoices(
        InvoiceFilter filter,
        SortFilter sort,
        PaginationFilter pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .Include(i => i.Items)
            .AsQueryable();

        query = ApplyFilters(query, filter);
        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sort);

        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var items = await query
            .Skip(skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Invoice> UpdateInvoiceAsync(
        int id,
        InvoiceUpdateDTO dto,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        // Tell EF the row version we *think* the row has. EF will include this in
        // the UPDATE's WHERE clause; if another transaction has bumped RowVersion
        // since the client GETed the row, the UPDATE affects 0 rows and EF throws
        // DbUpdateConcurrencyException.
        _context.Entry(invoice).Property(i => i.RowVersion).OriginalValue = dto.RowVersion;

        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.CustomerName = dto.CustomerName;
        invoice.CustomerBusinessId = dto.CustomerBusinessId;
        invoice.IssueDate = dto.IssueDate;
        invoice.DueDate = dto.DueDate;
        invoice.Status = dto.Status;
        invoice.CurrencyCode = dto.CurrencyCode;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return invoice;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

            if (databaseValues is null)
                throw new KeyNotFoundException($"Invoice {id} was deleted by another user.");

            var current = (Invoice)databaseValues.ToObject();
            throw new ConcurrencyConflictException(nameof(Invoice), id, current);
        }
    }

    private static IQueryable<Invoice> ApplyFilters(IQueryable<Invoice> query, InvoiceFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.InvoiceNumber))
            query = query.Where(i => i.InvoiceNumber.Contains(filter.InvoiceNumber));

        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
            query = query.Where(i => i.CustomerName.Contains(filter.CustomerName));

        if (!string.IsNullOrWhiteSpace(filter.CustomerBusinessId))
            query = query.Where(i => i.CustomerBusinessId == filter.CustomerBusinessId);

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status.Value);

        if (filter.IssueDateFrom.HasValue)
            query = query.Where(i => i.IssueDate >= filter.IssueDateFrom.Value);

        if (filter.IssueDateTo.HasValue)
            query = query.Where(i => i.IssueDate <= filter.IssueDateTo.Value);

        if (filter.DueDateFrom.HasValue)
            query = query.Where(i => i.DueDate >= filter.DueDateFrom.Value);

        if (filter.DueDateTo.HasValue)
            query = query.Where(i => i.DueDate <= filter.DueDateTo.Value);

        return query;
    }

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