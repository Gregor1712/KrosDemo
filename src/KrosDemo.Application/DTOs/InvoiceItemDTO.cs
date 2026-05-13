namespace KrosDemo.Application.DTOs;

public class InvoiceItemDTO
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public required string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public int InvoiceId { get; set; }

    public decimal NetAmount => Math.Round(Quantity * UnitPrice, 2);
    public decimal GrossAmount => Math.Round(NetAmount * (1 + VatRate / 100m), 2);
}