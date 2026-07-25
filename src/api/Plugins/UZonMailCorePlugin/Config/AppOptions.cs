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
        public DatabaseOptions DataBase { get; set; } = new();
        public HttpOptions Http { get; set; } = new();
        public LoggerOptions Logger { get; set; } = new();
        public UserOptions User { get; set; } = new();
        public WebsocketOptions Websocket { get; set; } = new();
        public TokenParams TokenParams { get; set; } = new();
        public FileStorageOptions FileStorage { get; set; } = new FileStorageOptions();
    }
}
