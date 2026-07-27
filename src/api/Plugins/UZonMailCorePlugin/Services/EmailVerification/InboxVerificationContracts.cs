using UzonMail.DB.SQL.Core.Emails;

namespace UzonMail.CorePlugin.Services.EmailVerification;

/// <summary>
/// 收件箱验证结论
/// </summary>
public enum InboxVerificationState
{
    NotChecked,
    Valid,
    Invalid,
    Unknown,
}

/// <summary>
/// 发起收件箱验证时所需的稳定输入
/// </summary>
public sealed record InboxVerificationRequest(long UserId, long InboxId, string Email);

/// <summary>
/// 单个验证器输出的强类型验证证据
/// </summary>
public abstract record InboxVerificationEvidence(
    InboxVerificationState State,
    string? FailureReason
);

/// <summary>
/// 收件箱的聚合验证报告
/// </summary>
public sealed record InboxVerificationReport(
    InboxVerificationRequest Request,
    InboxVerificationState State,
    IReadOnlyList<InboxVerificationEvidence> Evidences,
    string? FailureReason
);

/// <summary>
/// 收件箱验证器扩展点
/// </summary>
public interface IInboxVerifier
{
    /// <summary>
    /// 验证指定收件箱并返回强类型报告
    /// </summary>
    Task<InboxVerificationReport> VerifyAsync(
        InboxVerificationRequest request,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// 批量验证完成后的统计结果
/// </summary>
public sealed record InboxVerificationBatchSummary(
    int TotalCount,
    int ValidCount,
    int InvalidCount,
    int UnknownCount
);
