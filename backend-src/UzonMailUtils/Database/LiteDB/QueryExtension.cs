using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UZonMail.Utils.Web.RequestModel;

namespace UZonMail.Utils.Database.LiteDB
{
    internal static class QueryExtension
    {
        /// <summary>
        /// 获取分页数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pagination"></param>
        /// <param name="orderFunc"></param>
        /// <returns></returns>
        public static int GetPageDatasCount<T>(this IEnumerable<T> source, FilterModel filter) where T : AutoObjectId
        {
            var regex = new Regex(filter.Filter);

            // 进行筛选
            var results = source.Where(h => regex.IsMatch(h.GetFilterString()));

            return results.Count();
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pagination"></param>
        /// <param name="orderFunc"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetPageDatas<T>(this IEnumerable<T> source, FilterModel filter, PaginationModel pagination) where T : AutoObjectId
        {
            var regex = new Regex(filter.Filter);

            // 进行筛选
            var results = source.Where(h => regex.IsMatch(h.GetFilterString()));

            if (pagination.Descending)
            {
                results = results.OrderByDescending(item => item.GetValue(pagination.SortBy));
            }
            else
            {
                results = results.OrderBy(item => item.GetValue(pagination.SortBy));
            }

            return results.Skip(pagination.Skip).Take(pagination.Limit);
        }
    }
}
