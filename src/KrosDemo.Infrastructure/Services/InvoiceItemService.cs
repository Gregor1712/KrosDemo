using Microsoft.EntityFrameworkCore;
using KrosDemo.Application.DTOs;
using KrosDemo.Application.Exceptions;
using KrosDemo.Application.Interfaces;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Services;

public class InvoiceItemService : IInvoiceItemService
{
    private readonly ApplicationDbContext _context;

    public InvoiceItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceItem> UpdateInvoiceItemAsync(
        int id,
        InvoiceItemUpdateDTO dto,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.InvoiceItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"InvoiceItem {id} not found.");

        _context.Entry(item).Property(i => i.RowVersion).OriginalValue = rowVersion;

        item.Description = dto.Description;
        item.Unit = dto.Unit;
        item.Quantity = dto.Quantity;
        item.UnitPrice = dto.UnitPrice;
        item.VatRate = dto.VatRate;

        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return item;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

            if (databaseValues is null)
                throw new KeyNotFoundException($"InvoiceItem {id} was deleted by another user.");

            var current = (InvoiceItem)databaseValues.ToObject();
            throw new ConcurrencyConflictException(nameof(InvoiceItem), id, current);
        }
    }

    public async Task DeleteInvoiceItemAsync(
        int id,
        byte[] rowVersion,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.InvoiceItems.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"InvoiceItem {id} not found.");

        _context.Entry(item).Property(i => i.RowVersion).OriginalValue = rowVersion;
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
}