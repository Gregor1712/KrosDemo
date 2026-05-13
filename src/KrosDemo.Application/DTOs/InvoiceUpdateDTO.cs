using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.DTOs;

public class InvoiceUpdateDTO
{
    public required string InvoiceNumber { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerBusinessId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public required string CurrencyCode { get; set; }
    public required byte[] RowVersion { get; set; }
}