namespace UzonMail.DB.SQL.Core.Emails;

/// <summary>
/// 按邮箱域名维护的协议服务器信息。
/// </summary>
public interface IMailServerInfo
{
    string Domain { get; set; }
    string Host { get; set; }
    int Port { get; set; }
    ConnectionSecurity ConnectionSecurity { get; set; }
}
