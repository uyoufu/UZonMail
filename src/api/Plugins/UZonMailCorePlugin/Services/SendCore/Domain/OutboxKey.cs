namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public readonly record struct OutboxKey(long UserId, long OutboxId)
{
    public override string ToString() => $"{UserId}:{OutboxId}";
}
