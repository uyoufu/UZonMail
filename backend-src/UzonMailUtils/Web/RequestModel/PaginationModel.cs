using System;
using System.Collections.Generic;
using System.Text;

namespace UZonMail.Utils.Web.RequestModel
{
    /// <summary>
    /// 分页数据
    /// </summary>
    public class PaginationModel
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortBy { get; set; } = "_id";

        /// <summary>
        /// 是否降序
        /// </summary>
        public bool Descending { get; set; }=false;

        /// <summary>
        /// 跳过的记录数
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// 返回限制数量
        /// </summary>
        public int Limit { get; set; } = 20;
    }
}
