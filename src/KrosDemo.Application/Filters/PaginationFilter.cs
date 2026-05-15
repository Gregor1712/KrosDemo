using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace KrosDemo.Application.Filters;

    public class PaginationFilter
    {
        const int FIRST_PAGE_INDEX = 1;
        const int MAX_PAGE_SIZE = 500;

        [Range(FIRST_PAGE_INDEX, int.MaxValue)]
        public int? PageNumber { get; set; }

        [Range(0, MAX_PAGE_SIZE)]
        public int? PageSize { get; set; }

        public PaginationFilter() { }

        public PaginationFilter(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PaginationFilter CountOnly()
        {
            return new PaginationFilter(FIRST_PAGE_INDEX, 0);
        }

        public static PaginationFilter ForUniqueValues()
        {
            return new PaginationFilter();
        }

        public static PaginationFilter First()
        {
            return new PaginationFilter(FIRST_PAGE_INDEX, 1);
        }

        public IQueryable<TDBO> ApplyOrderById<TDBO>(IQueryable<TDBO> query)
        {
            var ordered = query.Expression?.Type == typeof(IOrderedQueryable<TDBO>);
            if (ordered)
            {
                return ApplyOrderBy<TDBO, int>((IOrderedQueryable<TDBO>)query, "Id");
            }
            return ApplyOrderBy<TDBO, int>(query, "Id");
        }

        public IQueryable<TDBO> ApplyOrderByNullableId<TDBO>(IQueryable<TDBO> query)
        {
            var ordered = query.Expression?.Type == typeof(IOrderedQueryable<TDBO>);
            if (ordered)
            {
                return ApplyOrderBy<TDBO, long?>((IOrderedQueryable<TDBO>)query, "Id");
            }
            return ApplyOrderBy<TDBO, long?>(query, "Id");
        }

        public IQueryable<TDBO> Apply<TDBO>(IQueryable<TDBO> query)
        {
            if (PageNumber == null || PageSize == null)
            {
                return query;
            }

            return query.Skip(((int)PageNumber - 1) * (int)PageSize)
                        .Take((int)PageSize);
        }

        public IQueryable<TDBO> ApplyOrderByCount<TDBO>(IQueryable<TDBO> query)
        {
            return ApplyOrderBy<TDBO, int>(query, "Count");
        }

        protected IQueryable<TDBO> ApplyOrderBy<TDBO, TType>(IQueryable<TDBO> query, string propertyName)
        {
            if (PageNumber == null || PageSize == null)
            {
                return query;
            }

            var expression = GetExpression<TDBO, TType>(propertyName);
            return query.OrderBy(expression)
                        .Skip(((int)PageNumber - 1) * (int)PageSize)
                        .Take((int)PageSize);
        }

        protected IQueryable<TDBO> ApplyOrderBy<TDBO, TType>(IOrderedQueryable<TDBO> query, string propertyName)
        {
            if (PageNumber == null || PageSize == null)
            {
                return query;
            }

            var expression = GetExpression<TDBO, TType>(propertyName);
            return query.ThenBy(expression)
                        .Skip(((int)PageNumber - 1) * (int)PageSize)
                        .Take((int)PageSize);
        }

        private Expression<Func<TDBO, TType>> GetExpression<TDBO, TType>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(TDBO), typeof(TDBO).Name);
            var name = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<TDBO, TType>>(name, new ParameterExpression[] { parameter });
        }
    }