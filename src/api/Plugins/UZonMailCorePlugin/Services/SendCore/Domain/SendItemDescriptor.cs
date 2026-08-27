namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public sealed record SendItemDescriptor(
    long Id,
    long SendingGroupId,
    long SenderAccountId,
    int TriedCount
);

public readonly record struct SendItemCursor(long SenderAccountId, long Id)
{
    public static SendItemCursor Start => new(long.MinValue, long.MinValue);
}
