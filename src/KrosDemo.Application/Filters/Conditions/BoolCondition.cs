using System.Linq.Expressions;
using System.Reflection;

namespace KrosDemo.Application.Filters.Conditions;

public class BoolCondition : ConditionBase<bool>, IConditionBuilder
{
    private static readonly MethodInfo ListContains = typeof(List<bool>).GetMethod(nameof(List<bool>.Contains), new Type[] { typeof(bool) })!;

    public BoolCondition(string instanceName, string? navigationProperty = null) 
        : base(instanceName, navigationProperty) { }
    
    protected override Expression BuildExpression(Expression name, List<bool> values, ConditionType? @operator)
    {
        Expression expression;
        Expression conversion;
        if (name.Type == typeof(bool))
            conversion = name;
        else
            conversion = Expression.Convert(name, typeof(bool));

        switch (@operator)
        {
            case ConditionType.Equals:
                expression = Expression.Equal(conversion, Expression.Constant(Values[0]));
                break;
            case ConditionType.GreaterThanOrEqual:
                expression = Expression.GreaterThanOrEqual(conversion, Expression.Constant(Values[0]));
                break;
            case ConditionType.LessThanOrEqual:
                expression = Expression.LessThanOrEqual(conversion, Expression.Constant(Values[0]));
                break;
            case ConditionType.In:
                expression = Expression.Call(Expression.Constant(Values), ListContains, conversion);
                break;
            default:
                throw new Exception("Invalid condition type " + @operator.ToString() + " for type string");
                //throw new InternalException("Invalid condition type " + @operator.ToString() + " for type bool");
        }

        return expression;
    }
}