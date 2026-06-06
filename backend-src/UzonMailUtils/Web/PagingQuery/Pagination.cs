using System.Linq;
using UZonMail.Utils.Web.RequestModel;

namespace UZonMail.Utils.Web.PagingQuery
{
    /// <summary>
    /// 获取分页数据
    /// </summary>
    public class Pagination : PaginationModel
    {
        public Pagination()
        {
            // 修改默认值
            SortBy = "Id";
        }

        /// <summary>
        /// 执行分页数据
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public IQueryable<T> Run<T>(IQueryable<T> values)
        {
            if (!string.IsNullOrEmpty(SortBy))
            {                
                values = values.OrderBy(SortBy, !Descending);
            }
            if (Skip > 0)
            {
                values = values.Skip(Skip);
            }
            if (Limit > 0)
            {
                values = values.Take(Limit);
            }
            return values;
        }
    }
}
