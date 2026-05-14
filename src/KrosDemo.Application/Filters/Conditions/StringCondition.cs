using System.Linq.Expressions;
using System.Reflection;

namespace KrosDemo.Application.Filters.Conditions;

    public class StringCondition : ConditionBase<string>, IConditionBuilder
    {
        private static readonly MethodInfo? StringContains = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
        private static readonly MethodInfo? StringStartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
        private static readonly MethodInfo? StringEndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });
        private static readonly MethodInfo? ListContains = typeof(List<string>).GetMethod(nameof(List<string>.Contains), new Type[] { typeof(string) });

        public StringCondition(string instanceName) : base(instanceName) { }

        protected override Expression BuildExpression(Expression name, List<string> values, ConditionType? @operator)
        {
            Expression expression;
            switch (@operator)
            {
                case ConditionType.Equals:
                    expression = Expression.Equal(name, Expression.Constant(Values[0]));
                    break;
                case ConditionType.NotEquals:
                    expression = Expression.Not(Expression.Equal(name, Expression.Constant(Values[0])));
                    break;
                case ConditionType.StartsWith:
                    expression = Expression.Call(name, StringStartsWith!, Expression.Constant(Values[0]));
                    break;
                case ConditionType.EndsWith:
                    expression = Expression.Call(name, StringEndsWith!, Expression.Constant(Values[0]));
                    break;
                case ConditionType.Contains:
                    expression = Expression.Call(name, StringContains!, Expression.Constant(Values[0]));
                    break;
                case ConditionType.In:
                    expression = Expression.Call(Expression.Constant(Values), ListContains!, name);
                    break;
                case ConditionType.NotIn:
                    expression = Expression.Not(Expression.Call(Expression.Constant(Values), ListContains!, name));
                    break;
                case ConditionType.IsNull:
                    expression = Expression.Call(typeof(string), nameof(string.IsNullOrWhiteSpace), null, name);
                    break;
                case ConditionType.IsNotNull:
                    expression = Expression.Not(Expression.Call(typeof(string), nameof(string.IsNullOrWhiteSpace), null, name));
                    break;
                default:
                    throw new Exception("Invalid condition type " + @operator.ToString() + " for type string");
            }

            return expression;
        }
    }