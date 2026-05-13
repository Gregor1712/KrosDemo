namespace KrosDemo.Domain.Entities;

public enum InvoiceStatus
{
    Issued = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}