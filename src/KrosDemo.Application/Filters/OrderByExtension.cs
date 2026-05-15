using System.Linq.Expressions;

namespace KrosDemo.Application.Filters;

    public static class OrderByExtension
    {
        public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> query, string propertyName, bool ascending)
        {
            var type = ReflectionHelper.GetPropertyType<T>(propertyName);
            if (type == typeof(bool) || type == typeof(Boolean))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, bool>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, bool>(propertyName));
            }
            else if (type == typeof(bool?))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, bool?>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, bool?>(propertyName));
            }
            else if (type == typeof(int))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, int>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, int>(propertyName));
            }
            else if (type == typeof(Int32?))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, Int32?>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, Int32?>(propertyName));
            }
            else if (type == typeof(string))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, string>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, string>(propertyName));
            }
            else if (type == typeof(DateTime))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, DateTime>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, DateTime>(propertyName));
            }
            else if (type == typeof(DateTime?))
            {
                return ascending ?
                    query.OrderBy(GetOrderKey<T, DateTime?>(propertyName)) :
                    query.OrderByDescending(GetOrderKey<T, DateTime?>(propertyName));
            }
            return query;
        }

        private static Expression<Func<TDBO, TType>> GetOrderKey<TDBO, TType>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TDBO));
            var body = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<TDBO, TType>>(body, parameter);
        }
    }