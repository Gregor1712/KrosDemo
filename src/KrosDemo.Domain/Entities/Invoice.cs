using CsvHelper.Configuration.Attributes;
using KrosDemo.Domain.Events;

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
    public byte[] RowVersion { get; set; } = [];

    [Ignore]
    public List<InvoiceItem> Items { get; set; } = [];

    public void MarkAsIssued()
    {
        Status = InvoiceStatus.Issued;
        AddDomainEvent(new InvoiceIssuedEvent(
            InvoiceNumber, CustomerName, CustomerBusinessId, DateTime.UtcNow));
    }

    public void Send()
    {
        if (Status != InvoiceStatus.Issued)
        {
            throw new InvalidOperationException(
                $"Invoice {InvoiceNumber} cannot be sent from status {Status}.");
        }

        Status = InvoiceStatus.Sent;
        AddDomainEvent(new InvoiceSentEvent(
            InvoiceNumber, CustomerName, CustomerBusinessId, DateTime.UtcNow));
    }
}