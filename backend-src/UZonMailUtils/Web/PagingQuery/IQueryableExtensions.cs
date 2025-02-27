using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UZonMail.Utils.Web.PagingQuery
{
    /// <summary>
    /// IEnumerable 扩展,用于分页排序
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// 根据字段名进行排序
        /// 不区分大小写
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="sortField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> list, string sortField, bool ascending)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = typeof(T).GetProperty(sortField, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
            {
                throw new ArgumentException("SortField is not a valid property");
            }
            var expr = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, prop), typeof(object)), param);
            return ascending ? list.OrderBy(expr) : list.OrderByDescending(expr);
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="values"></param>
        /// <param name="pick"></param>
        /// <returns></returns>
        public static IQueryable<T> Page<T>(this IQueryable<T> values, Pagination pick)
        {
            return pick.Run(values);
        }
    }
}
