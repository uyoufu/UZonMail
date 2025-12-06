using UZonMail.Utils.Web.Service;

namespace UZonMail.CorePlugin.Services.Config
{
    /// <summary>
    /// 调试配置
    /// </summary>
    public class DebugConfig : ISingletonService
    {
        /// <summary>
        /// 是否是示例状态
        /// 开启后，将不会真实发送邮件
        /// </summary>
        public bool IsDemo { get; set; }

        /// <summary>
        /// 是否阻止发送邮件
        /// </summary>
        public bool PreventSending { get; set; }

        public DebugConfig(IConfiguration configuration)
        {
            configuration.GetSection("Debug").Bind(this);
        }
    }
}
