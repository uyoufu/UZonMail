using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.DB.SQL.Settings
{
    /// <summary>
    /// 组织中的代理
    /// </summary>
    public class OrganizationProxy : OrgId
    {
        /// <summary>
        /// 代理名称
        /// </summary>
        public string Name { get; set; }

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
        /// 说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否生效
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 代理设置
        /// </summary>
        public string Proxy { get; set; }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsMatch(string outboxEmail)
        {
            if (string.IsNullOrEmpty(this.MatchRegex)) return true;

            try
            {
                var regex = new Regex(this.MatchRegex);
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
            if (string.IsNullOrEmpty(Proxy)) return null;
            return new ProxyInfo(Proxy);
        }
    }
}
