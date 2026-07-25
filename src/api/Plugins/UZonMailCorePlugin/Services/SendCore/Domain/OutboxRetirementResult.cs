namespace UzonMail.CorePlugin.Services.SendCore.Domain;

/// <summary>
/// 发件箱退出运行池后，当前邮件应采取的动作。
/// </summary>
public enum CurrentSendItemDisposition
{
    None,
    Fail,
    RetryWithAnotherOutbox,
}

/// <summary>
/// 发件箱退出流程的处理结果。
/// </summary>
public sealed record OutboxRetirementResult(CurrentSendItemDisposition CurrentItemDisposition);

/// <summary>
/// 根据当前邮件的发件箱绑定方式和剩余发件箱决定后续动作。
/// </summary>
public static class OutboxRetirementDecisionPolicy
{
    /// <summary>
    /// 计算发件箱退出后当前邮件应采取的动作。
    /// </summary>
    public static CurrentSendItemDisposition Decide(
        SendItemDescriptor? currentItem,
        bool hasAlternativeOutbox
    )
    {
        if (currentItem == null)
            return CurrentSendItemDisposition.None;
        if (currentItem.OutboxId > 0)
            return CurrentSendItemDisposition.Fail;
        return hasAlternativeOutbox
            ? CurrentSendItemDisposition.RetryWithAnotherOutbox
            : CurrentSendItemDisposition.Fail;
    }
}
