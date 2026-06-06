namespace UZonMail.CorePlugin.Config.SubConfigs
{
    public class MicrosoftEntraAppConfig
    {
        /// <summary>
        /// Microsoft Entra 应用程序 ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Microsoft Entra 租户 ID
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Microsoft Entra 应用程序密钥
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
