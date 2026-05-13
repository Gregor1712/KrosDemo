using CsvHelper.Configuration.Attributes;

namespace KrosDemo.Domain.Entities;

public class Invoice : BaseEntity
{
    public required string InvoiceNumber { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerBusinessId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public required string CurrencyCode { get; set; }

    [Ignore]
    public List<InvoiceItem> Items { get; set; } = [];
}