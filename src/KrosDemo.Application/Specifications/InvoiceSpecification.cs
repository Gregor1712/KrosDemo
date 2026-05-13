using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Specifications;

public class InvoiceSpecification : BaseSpecification<Invoice>
{
    public InvoiceSpecification(InvoiceSpecParams specParams)
        : base(i =>
            (string.IsNullOrEmpty(specParams.Search) || i.InvoiceNumber.ToLower().Contains(specParams.Search) || i.CustomerName.ToLower().Contains(specParams.Search)) &&
            (!specParams.Status.HasValue || i.Status == specParams.Status.Value) &&
            (string.IsNullOrEmpty(specParams.CustomerName) || i.CustomerName.Contains(specParams.CustomerName)))
    {
        ApplyPaging(specParams.PageSize * (specParams.PageIndex - 1), specParams.PageSize);

        AddInclude(i => i.Items);

        switch (specParams.Sort)
        {
            case "issueAsc":
                AddOrderBy(i => i.IssueDate);
                break;
            case "issueDesc":
                AddOrderByDescending(i => i.IssueDate);
                break;
            case "dueAsc":
                AddOrderBy(i => i.DueDate);
                break;
            case "dueDesc":
                AddOrderByDescending(i => i.DueDate);
                break;
            default:
                AddOrderByDescending(i => i.IssueDate);
                break;
        }
    }
}