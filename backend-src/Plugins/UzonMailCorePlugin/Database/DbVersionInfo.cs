namespace UZonMail.CorePlugin.Database
{
    public class DbVersionInfo
    {
        /// <summary>
        /// 当前需要的数据库版本
        /// </summary>
        public static Version RequiredVersion { get; } = new("0.15.3");

        public static string VersionSettingKey { get; } = "DataVersion";
    }
}
