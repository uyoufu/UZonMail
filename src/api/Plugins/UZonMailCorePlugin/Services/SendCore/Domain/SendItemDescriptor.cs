namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public sealed record SendItemDescriptor(
    long Id,
    long SendingGroupId,
    long OutboxId,
    int TriedCount
);

public readonly record struct SendItemCursor(long OutboxId, long Id)
{
    public static SendItemCursor Start => new(long.MinValue, long.MinValue);
}
