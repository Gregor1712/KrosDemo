using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace KrosDemo.Application.Filters;

public class PaginationFilter
{
    private const int FirstPageIndex = 1;
    private const int MaxPageSize = 500;

    [Range(FirstPageIndex, int.MaxValue)]
    public int? PageNumber { get; set; }

    [Range(0, MaxPageSize)]
    public int? PageSize { get; set; }

    public PaginationFilter() { }

    public PaginationFilter(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PaginationFilter CountOnly() => new(FirstPageIndex, 0);

    public static PaginationFilter ForUniqueValues() => new();

    public static PaginationFilter First() => new(FirstPageIndex, 1);

    public IQueryable<TDbo> Apply<TDbo>(IQueryable<TDbo> query)
    {
        if (PageNumber is not { } page || PageSize is not { } size)
        {
            return query;
        }

        return query.Skip((page - 1) * size).Take(size);
    }

    public IQueryable<TDbo> ApplyOrderById<TDbo>(IQueryable<TDbo> query) =>
        ApplyOrderBy<TDbo, int>(query, "Id");

    public IQueryable<TDbo> ApplyOrderByNullableId<TDbo>(IQueryable<TDbo> query) =>
        ApplyOrderBy<TDbo, long?>(query, "Id");

    public IQueryable<TDbo> ApplyOrderByCount<TDbo>(IQueryable<TDbo> query) =>
        ApplyOrderBy<TDbo, int>(query, "Count");

    private IQueryable<TDbo> ApplyOrderBy<TDbo, TKey>(IQueryable<TDbo> query, string propertyName)
    {
        if (PageNumber is not { } page || PageSize is not { } size)
        {
            return query;
        }

        var keySelector = BuildKeySelector<TDbo, TKey>(propertyName);
        var ordered = query is IOrderedQueryable<TDbo> alreadyOrdered
            ? alreadyOrdered.ThenBy(keySelector)
            : query.OrderBy(keySelector);

        return ordered.Skip((page - 1) * size).Take(size);
    }

    private static Expression<Func<TDbo, TKey>> BuildKeySelector<TDbo, TKey>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(TDbo), typeof(TDbo).Name);
        var property = Expression.Property(parameter, propertyName);
        return Expression.Lambda<Func<TDbo, TKey>>(property, parameter);
    }
}