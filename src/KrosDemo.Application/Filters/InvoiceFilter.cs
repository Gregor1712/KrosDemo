using KrosDemo.Application.Filters.Conditions;
using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Filters;

public class InvoiceFilter : FilterBase
{
    public StringCondition? InvoiceNumber { get; set; } = new(nameof(InvoiceNumber));
    public StringCondition? CustomerName { get; set; } = new(nameof(CustomerName));
    public StringCondition? CustomerBusinessId { get; set; } = new(nameof(CustomerBusinessId));
    public DateTimeCondition IssueDateFrom { get; set; } = new(nameof(IssueDateFrom));
    public DateTimeCondition IssueDateTo { get; set; } = new(nameof(IssueDateTo));
    public DateTimeCondition DueDateFrom { get; set; } = new(nameof(DueDateFrom));
    public DateTimeCondition DueDateTo { get; set; } = new(nameof(DueDateTo));
    public StringCondition? Description { get; set; } = new(nameof(Description), nameof(Invoice.Items));
}