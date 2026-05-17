using System.Linq.Expressions;
using System.Reflection;

namespace KrosDemo.Application.Filters.Conditions;

    public class IntCondition : ConditionBase<int>, IConditionBuilder
    {
        private static readonly MethodInfo ListContains = typeof(List<int>).GetMethod(nameof(List<int>.Contains), new Type[] { typeof(int) })!;
        
        public IntCondition(string instanceName, string? navigationProperty = null) 
            : base(instanceName, navigationProperty) { }
        
        public IntCondition(string instanceName, int number, ConditionType @operator = ConditionType.Equals) : base(instanceName)
        {
            Operator = @operator;
            Values = new List<int> { number };
        }

        protected override Expression BuildExpression(Expression name, List<int> values, ConditionType? @operator)
        {
            Expression expression;
            Expression conversion;
            if (name.Type == typeof(int))
                conversion = name;
            else
                conversion = Expression.Convert(name, typeof(int));

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
                case ConditionType.NotIn:
                    expression = Expression.Not(Expression.Call(Expression.Constant(Values), ListContains, conversion));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
                    //throw new InternalException("Invalid condition type " + @operator.ToString() + " for type int");
            }

            return expression;
        }
    }
