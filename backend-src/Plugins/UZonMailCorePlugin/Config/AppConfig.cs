using UZonMail.Core.Config.SubConfigs;
using UZonMail.Utils.Web.Token;

namespace UZonMail.Core.Config
{
    /// <summary>
    /// 程序所有的配置
    /// </summary>
    public class AppConfig()
    {
        public DatabaseConfig DataBase { get; set; }
        public HttpConfig Http { get; set; }
        public LoggerConfig Logger { get; set; }
        public UserConfig User { get; set; }
        public WebsocketConfig Websocket { get; set; }
        public TokenParams TokenParams { get; set; }
        public FileStorageConfig FileStorage { get; set; } = new FileStorageConfig();
    }
}
