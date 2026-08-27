using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Credentials;

/// <summary>
/// 使用带认证的 AES-GCM 保护账户密码和 OAuth 密钥。
/// </summary>
public sealed class CredentialProtector(IOptions<CredentialEncryptionOptions> options)
    : ICredentialProtector,
        ISingletonService<ICredentialProtector>
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly CredentialEncryptionOptions _options = Validate(options.Value);

    public string ActiveKeyVersion => _options.ActiveKeyVersion;

    public ProtectedCredential Protect(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var keyVersion = _options.ActiveKeyVersion;
        var key = GetKey(keyVersion);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var payload = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(payload, 0);
        tag.CopyTo(payload, NonceSize);
        ciphertext.CopyTo(payload, NonceSize + TagSize);
        return new ProtectedCredential(Convert.ToBase64String(payload), keyVersion);
    }

    public string Unprotect(string ciphertext, string keyVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyVersion);

        var payload = Convert.FromBase64String(ciphertext);
        if (payload.Length <= NonceSize + TagSize)
            throw new CryptographicException("凭证密文格式无效。");

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(NonceSize, TagSize);
        var encryptedBytes = payload.AsSpan(NonceSize + TagSize);
        var plaintext = new byte[encryptedBytes.Length];
        using var aes = new AesGcm(GetKey(keyVersion), TagSize);
        aes.Decrypt(nonce, encryptedBytes, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }

    private byte[] GetKey(string keyVersion)
    {
        if (!_options.Keys.TryGetValue(keyVersion, out var encodedKey))
            throw new CryptographicException($"未配置凭证密钥版本 {keyVersion}。");

        var key = Convert.FromBase64String(encodedKey);
        if (key.Length is not (16 or 24 or 32))
            throw new CryptographicException($"凭证密钥版本 {keyVersion} 的长度无效。");
        return key;
    }

    private static CredentialEncryptionOptions Validate(CredentialEncryptionOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ActiveKeyVersion))
            throw new InvalidOperationException("CredentialEncryption:ActiveKeyVersion 未配置。");
        if (!options.Keys.ContainsKey(options.ActiveKeyVersion))
            throw new InvalidOperationException("活动凭证密钥版本不存在于密钥环中。");
        return options;
    }
}
