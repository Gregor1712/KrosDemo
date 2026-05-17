using System.Linq.Expressions;

namespace KrosDemo.Application.Filters.Conditions;

    public class DateTimeCondition : ConditionBase<DateTime>, IConditionBuilder
    {
        public DateTimeCondition(string instanceName, string? navigationProperty = null)
            : base(instanceName, navigationProperty) { }

        protected override Expression BuildExpression(Expression name, List<DateTime> values, ConditionType? @operator)
        {
            var left = name;

            Expression right1 = Expression.Constant(new DateTime?());
            if (Values.Count > 0)
            {
                right1 = Expression.Constant(Values[0]);
            }

            Expression? right2 = null;
            if (Values.Count > 1)
            {
                right2 = Expression.Constant(Values[1]);
            }

            if (left.Type == typeof(DateTime?))
            {
                var leftValue = Expression.Property(left, "Value");
                var nullValue = Expression.Constant(null, typeof(DateTime?));
                left = Expression.Condition(
                    Expression.Equal(left, nullValue),
                    nullValue,
                    Expression.Convert(Expression.Property(leftValue, "Date"), typeof(DateTime?))
                );
                right1 = Expression.Convert(right1, typeof(DateTime?));
                if (right2 is not null)
                {
                    right2 = Expression.Convert(right2, typeof(DateTime?));
                }
            }

            Expression expression;
            switch (@operator)
            {
                case ConditionType.Equals:
                    expression = Expression.Equal(left, right1);
                    break;
                case ConditionType.Between:
                    if (right1 is null || right2 is null)
                    {
                        throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
                        //throw new InternalException("Invalid number of operands for " + @operator.ToString() + " for type datetime");
                    }
                    var expression1 = Expression.GreaterThanOrEqual(left, right1);
                    var expression2 = Expression.LessThan(left, right2);
                    expression = Expression.And(expression1, expression2);
                    break;
                case ConditionType.GreaterThanOrEqual:
                    expression = Expression.GreaterThanOrEqual(left, right1);
                    break;
                case ConditionType.GreaterThan:
                    expression = Expression.GreaterThan(left, right1);
                    break;
                case ConditionType.LessThanOrEqual:
                    expression = Expression.LessThanOrEqual(left, right1);
                    break;
                case ConditionType.LessThan:
                    expression = Expression.LessThan(left, right1);
                    break;
                case ConditionType.IsNull:
                    expression = Expression.Equal(left, right1);
                    break;
                case ConditionType.IsNotNull:
                    expression = Expression.Not(Expression.Equal(left, right1));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
                    //throw new InternalException("Invalid condition type " + @operator.ToString() + " for type datetime");
            }

            return expression;
        }
    }