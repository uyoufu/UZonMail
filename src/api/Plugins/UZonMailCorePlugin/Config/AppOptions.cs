using UzonMail.CorePlugin.Config.SubConfigs;
using UzonMail.Utils.Web.Configs;
using UzonMail.Utils.Web.Token;

namespace UzonMail.CorePlugin.Config
{
    /// <summary>
    /// 程序所有的配置
    /// </summary>
    public class AppOptions
    {
        public DatabaseOptions DataBase { get; set; }
        public HttpOptions Http { get; set; }
        public LoggerOptions Logger { get; set; }
        public UserOptions User { get; set; }
        public WebsocketOptions Websocket { get; set; }
        public TokenParams TokenParams { get; set; }
        public FileStorageOptions FileStorage { get; set; } = new FileStorageOptions();
    }
}
