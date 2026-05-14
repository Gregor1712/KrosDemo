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

    public ConditionBase(string name)
    {
        Name = name;
    }

    public Expression? BuildExpression(ParameterExpression parameter)
    {
        if (Operator != ConditionType.IsNull && Operator != ConditionType.IsNotNull && Values.Count == 0)
        {
            return null;
        }

        var name = Expression.Property(parameter, Name);
        return BuildExpression(name, Values, Operator);
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