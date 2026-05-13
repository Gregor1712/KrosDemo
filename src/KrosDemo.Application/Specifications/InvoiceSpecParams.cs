using KrosDemo.Domain.Entities;

namespace KrosDemo.Application.Specifications;

public class InvoiceSpecParams : PagingParams
{
    private string? _search;
    public string Search
    {
        get => _search ?? "";
        set => _search = value.ToLower();
    }

    public InvoiceStatus? Status { get; set; }
    public string? CustomerName { get; set; }
    public string? Sort { get; set; }
}