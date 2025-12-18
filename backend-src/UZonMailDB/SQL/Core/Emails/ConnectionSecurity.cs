using MailKit.Security;

namespace UZonMail.DB.SQL.Core.Emails
{
    public enum ConnectionSecurity
    {
        None,
        SSL,
        TLS,
        StartTLS
    }

    public static class ConnectionSecurityExtensions
    {
        /// <summary>
        /// 将 ConnectionSecurity 转换为 MailKit 的 SecureSocketOptions
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public static SecureSocketOptions ToMailKitSecureSocketOptions(
            this ConnectionSecurity security
        )
        {
            return security switch
            {
                ConnectionSecurity.None => SecureSocketOptions.None,
                ConnectionSecurity.SSL => SecureSocketOptions.SslOnConnect,
                ConnectionSecurity.TLS => SecureSocketOptions.SslOnConnect,
                ConnectionSecurity.StartTLS => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto,
            };
        }
    }
}
