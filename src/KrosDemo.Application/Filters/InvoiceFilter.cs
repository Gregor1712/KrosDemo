using KrosDemo.Application.Filters.Conditions;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Filters;

public class InvoiceFilter : FilterBase
{
    public StringCondition? InvoiceNumber { get; set; } = new(nameof(InvoiceNumber));
    public StringCondition? CustomerName { get; set; } = new(nameof(CustomerName));
    public StringCondition? CustomerBusinessId { get; set; } = new(nameof(CustomerBusinessId));
    public DateTimeCondition IssueDate { get; set; } = new(nameof(IssueDate));
    public DateTimeCondition DueDate { get; set; } = new(nameof(DueDate));
    public StringCondition? Description { get; set; } = new(nameof(Description), nameof(Invoice.Items));
}