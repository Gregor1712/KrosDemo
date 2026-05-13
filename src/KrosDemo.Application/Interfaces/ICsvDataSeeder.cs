namespace KrosDemo.Application.Interfaces;

public interface ICsvDataSeeder
{
    Task SeedDataAsync();
    Task SeedInvoicesAsync();
    Task SeedInvoiceItemsAsync();
}