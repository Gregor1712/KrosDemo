using CsvHelper.Configuration.Attributes;

namespace KrosDemo.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public required string Description { get; set; }
    public required string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }

    public int InvoiceId { get; set; }

    [Ignore]
    public Invoice Invoice { get; set; } = null!;
}