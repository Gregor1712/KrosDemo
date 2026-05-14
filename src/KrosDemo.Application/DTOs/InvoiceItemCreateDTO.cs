namespace KrosDemo.Application.DTOs;

public class InvoiceItemCreateDTO
{
    public required string Description { get; set; }
    public required string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
}