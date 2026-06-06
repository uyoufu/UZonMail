using System.Collections.Generic;

namespace UZonMail.Utils.Web.RequestModel
{
    public class FilterModel
    {
        public string Filter { get; set; } = string.Empty;

        /// <summary>
        /// 过滤的字段名称，每个字段名称之间是通过 Or 进行连接的
        /// </summary>
        public List<string> FieldNames { get; set; }=new List<string>();
    }
}
