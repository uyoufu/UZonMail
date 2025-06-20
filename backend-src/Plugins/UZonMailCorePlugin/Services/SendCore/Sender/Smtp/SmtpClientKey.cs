namespace UZonMail.Core.Services.SendCore.Sender.Smtp
{
    /// <summary>
    /// SmtpClient 的 Key 对象
    /// </summary>
    public class SmtpClientKey(string email, string proxy)
    {
        public string Email => email;
        public string Proxy => proxy;

        /// <summary>
        /// 包含代理
        /// </summary>
        public bool HasProxy => !string.IsNullOrEmpty(proxy);

        public override bool Equals(object? obj)
        {
            if (obj is not SmtpClientKey other) return false;

            return Email == other.Email &&
                   Proxy == other.Proxy;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Email, Proxy);
        }
    }
}
