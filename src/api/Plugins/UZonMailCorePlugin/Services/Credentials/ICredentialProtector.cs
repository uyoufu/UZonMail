namespace UzonMail.CorePlugin.Services.Credentials;

public sealed record ProtectedCredential(string Ciphertext, string KeyVersion);

/// <summary>
/// 邮件账户凭证的唯一加解密边界。
/// </summary>
public interface ICredentialProtector
{
    string ActiveKeyVersion { get; }
    ProtectedCredential Protect(string plaintext);
    string Unprotect(string ciphertext, string keyVersion);
}
