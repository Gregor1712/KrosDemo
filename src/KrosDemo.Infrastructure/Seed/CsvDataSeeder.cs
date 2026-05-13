using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using KrosDemo.Application.Interfaces;
using KrosDemo.Domain.Entities;
using KrosDemo.Infrastructure.Data;

namespace KrosDemo.Infrastructure.Seed;

public class CsvDataSeeder : ICsvDataSeeder
{
    private readonly ApplicationDbContext _context;

    public CsvDataSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedDataAsync()
    {
        if (_context.Invoices.Any())
        {
            Console.WriteLine("Database already seeded.");
            return;
        }

        await SeedInvoicesAsync();
        await SeedInvoiceItemsAsync();

        Console.WriteLine("Database seeded successfully!");
    }

    public async Task SeedInvoicesAsync()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };

        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "Seed", "Csv", "invoices.csv"));
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<Invoice>().ToList();

        _context.Invoices.AddRange(records);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine($"Seeded {records.Count} invoices.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error seeding invoices: {ex.Message}");
            throw;
        }
    }

    public async Task SeedInvoiceItemsAsync()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";"
        };

        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "Seed", "Csv", "invoice-items.csv"));
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<InvoiceItem>().ToList();

        _context.InvoiceItems.AddRange(records);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Console.WriteLine($"Seeded {records.Count} invoice items.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error seeding invoice items: {ex.Message}");
            throw;
        }
    }
}