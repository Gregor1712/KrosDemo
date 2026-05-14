using KrosDemo.Application.Filters.Conditions;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Filters;

public class InvoiceFilter : FilterBase
{
    public StringCondition? CustomerName { get; set; } = new(nameof(CustomerName));
    // public string? InvoiceNumber { get; set; }
    // public string? CustomerName { get; set; }
    // public string? CustomerBusinessId { get; set; }
    // public InvoiceStatus? Status { get; set; }
    // public DateTime? IssueDateFrom { get; set; }
    // public DateTime? IssueDateTo { get; set; }
    // public DateTime? DueDateFrom { get; set; }
    // public DateTime? DueDateTo { get; set; }
}