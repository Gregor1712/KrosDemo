namespace KrosDemo.Application.Filters;

public class SortFilter
{
    public string? SortBy { get; set; }
    public SortDirection Direction { get; set; } = SortDirection.Asc;
}

public enum SortDirection
{
    Asc,
    Desc
}