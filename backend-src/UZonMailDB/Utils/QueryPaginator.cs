namespace UZonMail.DB.Utils
{
    /// <summary>
    /// 分页加载器
    /// </summary>
    public class QueryPaginator<T>(IQueryable<T> query, int pageSize = 100) where T : class
    {
        private int _skip = 0;
        private int _take = pageSize;

        /// <summary>
        /// 获取当前页数据
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> GetPage()
        {
            var results = query.Skip(_skip).Take(_take);
            _skip += _take;
            return results;
        }
        /// <summary>
        /// 设置分页大小
        /// </summary>
        /// <param name="pageIndex"></param>
        public void SetPageSize(int pageSize)
        {
            _take = pageSize;
        }
    }
}
