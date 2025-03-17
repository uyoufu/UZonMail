using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Core.Emails;

namespace UZonMail.DB.SQL.Core.Settings
{
    /// <summary>
    /// 组织中的代理
    /// 若有组织 id, 则组织共用，若没有组织 id, 则为个人代理
    /// </summary>
    public class Proxy : UserAndOrgId
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 优先级
        /// 值越大，优先级越高，越先匹配
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 邮件匹配规则
        /// 使用正则表达式
        /// </summary>
        public string? MatchRegex { get; set; }

        /// <summary>
        /// 是否生效
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 是否共享
        /// 主要用于前端显示，实际上是根据组织 id 判断
        /// 当修改共享状态时，需要同步更新组织 id
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// 代理设置地址
        /// 格式：http://username:password@host:port
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsMatch(string outboxEmail)
        {
            if (string.IsNullOrEmpty(MatchRegex)) return true;

            try
            {
                var regex = new Regex(MatchRegex);
            }
            catch
            {
                // 说明正则表达式有问题
                return false;
            }

            return Regex.IsMatch(outboxEmail, MatchRegex);
        }

        /// <summary>
        /// 转换成代理信息类
        /// </summary>
        /// <returns></returns>
        public ProxyInfo? ToProxyInfo()
        {
            if (string.IsNullOrEmpty(Url)) return null;
            return new ProxyInfo(Url);
        }
    }
}
