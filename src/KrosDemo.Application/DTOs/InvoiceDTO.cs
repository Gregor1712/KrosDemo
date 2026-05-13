using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.DTOs;

public class InvoiceDTO
{
    public int Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerBusinessId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public required string CurrencyCode { get; set; }
    public List<InvoiceItemDTO> Items { get; set; } = [];

    public decimal TotalNet => Items.Sum(i => i.NetAmount);
    public decimal TotalGross => Items.Sum(i => i.GrossAmount);
}