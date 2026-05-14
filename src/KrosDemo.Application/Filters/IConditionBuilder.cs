using System.Linq.Expressions;

namespace KrosDemo.Application.Filters;

public interface IConditionBuilder
{
    Expression BuildExpression(ParameterExpression parameter);
}