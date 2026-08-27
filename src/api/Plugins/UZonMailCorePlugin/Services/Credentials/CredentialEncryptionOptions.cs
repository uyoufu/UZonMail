using UzonMail.Utils.Web.Configs;

namespace UzonMail.CorePlugin.Services.Credentials;

/// <summary>
/// 凭证密钥环。旧版本密钥仅用于解密，新增密文始终使用活动版本。
/// </summary>
public sealed class CredentialEncryptionOptions : IAppOptions
{
    public string ActiveKeyVersion { get; set; } = string.Empty;
    public Dictionary<string, string> Keys { get; set; } = [];
}
