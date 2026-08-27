namespace UzonMail.CorePlugin.Services.SendCore.Domain;

/// <summary>
/// 一次发送尝试完成后的提交动作。
/// </summary>
public enum SendAttemptDisposition
{
    Succeeded,
    Failed,
    Retry,
    Release,
}

/// <summary>
/// 发送结果经过业务规则分类后的提交决策。
/// </summary>
public sealed record SendAttemptDecision(
    SendAttemptDisposition Disposition,
    string Message,
    string? ReceiptId = null
)
{
    public bool IsTerminal =>
        Disposition is SendAttemptDisposition.Succeeded or SendAttemptDisposition.Failed;
}

/// <summary>
/// 将 Transport 结果转换为队列和数据库可以执行的提交决策。
/// </summary>
public static class SendAttemptDecisionPolicy
{
    /// <summary>
    /// 根据 Transport 结果、重试次数和发件箱退出结果生成提交决策。
    /// </summary>
    public static SendAttemptDecision Decide(
        TransportResult transportResult,
        int triedCount,
        int maxRetryCount,
        SenderAccountRetirementResult? senderAccountRetirement
    )
    {
        if (transportResult.IsSuccess)
        {
            return new SendAttemptDecision(
                SendAttemptDisposition.Succeeded,
                transportResult.ReceiptId ?? transportResult.Message,
                transportResult.ReceiptId
            );
        }

        if (transportResult.FailureKind == SendFailureKind.Cancelled)
            return new SendAttemptDecision(SendAttemptDisposition.Release, transportResult.Message);

        var isPermanentItemFailure =
            transportResult.FailureKind
                is SendFailureKind.HardBounce
                    or SendFailureKind.RecipientPermanent
                    or SendFailureKind.MessagePermanent
                    or SendFailureKind.LocalData;
        if (isPermanentItemFailure)
            return new SendAttemptDecision(SendAttemptDisposition.Failed, transportResult.Message);

        if (transportResult.FailureKind == SendFailureKind.SenderAccountPermanent)
        {
            if (
                senderAccountRetirement?.CurrentItemDisposition
                != CurrentSendItemDisposition.RetryWithAnotherSenderAccount
            )
                return new SendAttemptDecision(
                    SendAttemptDisposition.Failed,
                    transportResult.Message
                );
        }

        if (triedCount >= maxRetryCount)
        {
            return new SendAttemptDecision(
                SendAttemptDisposition.Failed,
                $"当前邮箱重试已达最大次数 {maxRetryCount}"
            );
        }

        return new SendAttemptDecision(SendAttemptDisposition.Retry, transportResult.Message);
    }
}
