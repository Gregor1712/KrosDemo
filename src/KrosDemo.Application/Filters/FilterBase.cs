using System.Linq.Expressions;

namespace KrosDemo.Application.Filters;

public static class FilterExtensions
{
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, FilterBase filter)
    {
        if (filter == null) return query;

        return filter.Apply(query);
    }
}

public class FilterBase
{
    public IQueryable<T> Apply<T>(IQueryable<T> query)
    {
        var parameter = Expression.Parameter(typeof(T), typeof(T).Name);

        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            var condition = property.GetValue(this) as IConditionBuilder;
            var expression = condition?.BuildExpression(parameter);
            if (expression == null)
                continue;

            var lambda = Expression.Lambda<Func<T, bool>>(expression, parameter);
            if (lambda == null)
                continue;

            query = query.Where(lambda);
        }

        return query;
    }
}