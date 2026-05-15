using System.Text.Json.Serialization;

namespace KrosDemo.Application.Filters;

public class SortFilter
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortType
    {
        Ascending = 0,
        Descending
    }

    public string SortProperty { get; set; }
    public SortType SortDirection { get; set; } = SortType.Ascending;

    public static SortFilter None()
    {
        return new();
    }

    public IQueryable<T> Apply<T>(IQueryable<T> query)
    {
        if (SortProperty == null)
        {
            return query;
        }

        return query.ApplyOrderBy(SortProperty, SortDirection == SortType.Ascending);
    }
}

