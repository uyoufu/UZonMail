using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;

namespace UzonMail.DB.SQL.Core.EmailReceiving;

/// <summary>
/// IMAP 账户的敏感认证信息。
/// 密钥和令牌仅保存加密后的密文，解密职责属于业务层。
/// </summary>
public class ImapAccountCredential : SqlId, IEntityTypeConfiguration<ImapAccountCredential>
{
    /// <summary>
    /// 所属 IMAP 账户的数据库标识。
    /// </summary>
    public long ImapAccountId { get; set; }

    /// <summary>
    /// 所属 IMAP 账户。
    /// </summary>
    public ImapAccount ImapAccount { get; set; } = null!;

    /// <summary>
    /// 服务器认证使用的登录名，可能与收件邮箱地址不同。
    /// </summary>
    public string LoginName { get; set; } = string.Empty;

    /// <summary>
    /// 经业务层加密后的密码；仅密码认证方式使用。
    /// </summary>
    public string? EncryptedPassword { get; set; }

    /// <summary>
    /// OAuth 令牌的服务提供商；仅 OAuth2 认证方式使用。
    /// </summary>
    public OAuthProviderType OAuthProvider { get; set; }

    /// <summary>
    /// 经业务层加密后的 OAuth 访问令牌。
    /// </summary>
    public string? EncryptedOAuthAccessToken { get; set; }

    /// <summary>
    /// 经业务层加密后的 OAuth 刷新令牌。
    /// </summary>
    public string? EncryptedOAuthRefreshToken { get; set; }

    /// <summary>
    /// OAuth 访问令牌过期的 UTC 时间。
    /// </summary>
    public DateTime? OAuthAccessTokenExpiresAtUtc { get; set; }

    /// <summary>
    /// OAuth 客户端标识；仅在账户级配置时保存。
    /// </summary>
    public string? OAuthClientId { get; set; }

    /// <summary>
    /// 经业务层加密后的 OAuth 客户端密钥。
    /// </summary>
    public string? EncryptedOAuthClientSecret { get; set; }

    /// <summary>
    /// 加密密文使用的密钥版本，支持后续密钥轮换。
    /// </summary>
    public string EncryptionKeyVersion { get; set; } = string.Empty;

    /// <summary>
    /// 凭据最近一次更新的 UTC 时间。
    /// </summary>
    public DateTime CredentialUpdatedAtUtc { get; set; }

    /// <summary>
    /// OAuth 租户标识，例如 Microsoft Entra 租户。
    /// </summary>
    public string? OAuthTenantId { get; set; }

    /// <summary>
    /// 自定义 OAuth 提供商的令牌端点地址。
    /// </summary>
    public string? OAuthTokenEndpoint { get; set; }

    /// <summary>
    /// 配置账户凭据与收件账户的一对一关系。
    /// </summary>
    public void Configure(EntityTypeBuilder<ImapAccountCredential> builder)
    {
        builder.ToTable("ImapAccountCredentials");
        builder.Property(x => x.LoginName).HasMaxLength(320).IsRequired();
        builder.Property(x => x.OAuthClientId).HasMaxLength(500);
        builder.Property(x => x.EncryptionKeyVersion).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OAuthTenantId).HasMaxLength(500);
        builder.Property(x => x.OAuthTokenEndpoint).HasMaxLength(2000);
        builder.HasIndex(x => x.ImapAccountId).IsUnique();
        builder
            .HasOne(x => x.ImapAccount)
            .WithOne(x => x.Credential)
            .HasForeignKey<ImapAccountCredential>(x => x.ImapAccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
