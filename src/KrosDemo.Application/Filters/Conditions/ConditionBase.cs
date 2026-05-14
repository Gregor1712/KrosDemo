using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace KrosDemo.Application.Filters.Conditions;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionType
{
    Equals,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Between,
    StartsWith,
    Contains,
    In,
    NotIn,
    NotEquals, 
    EndsWith,
    IsNull,
    IsNotNull
}

public class ConditionBase<T>
{
    public ConditionType? Operator { get; set; } = ConditionType.Equals;
    public List<T> Values { get; set; } = new();
    private string Name { get; }
    private string? NavigationProperty { get; }

    public ConditionBase(string name, string? navigationProperty = null)
    {
        Name = name;
        NavigationProperty = navigationProperty;
    }

    public Expression? BuildExpression(ParameterExpression parameter)
    {
        if (Operator != ConditionType.IsNull && Operator != ConditionType.IsNotNull && Values.Count == 0)
        {
            return null;
        }

        if (NavigationProperty is null)
        {
            var property = Expression.Property(parameter, Name);
            return BuildExpression(property, Values, Operator);
        }

        // Navigate to collection: e.g. x.Items
        var collection = Expression.Property(parameter, NavigationProperty);

        // Get the element type of the collection (e.g. InvoiceItem)
        var elementType = collection.Type.GetGenericArguments()[0];

        // Build: item => item.Description.Contains("value")
        var childParam = Expression.Parameter(elementType, "child");
        var childProperty = Expression.Property(childParam, Name);
        var childCondition = BuildExpression(childProperty, Values, Operator);
        var childLambda = Expression.Lambda(childCondition, childParam);

        // Build: x.Items.Any(item => item.Description.Contains("value"))
        var anyMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 2)
            .MakeGenericMethod(elementType);

        return Expression.Call(anyMethod, collection, childLambda);
    }

    protected virtual Expression BuildExpression(Expression name, List<T> values, ConditionType? @operator)
    {
        throw new NotImplementedException();
    }

    public bool IsEmpty()
    {
        return Values.Count == 0;
    }
}