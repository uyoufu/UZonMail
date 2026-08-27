namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public readonly record struct SenderAccountKey(long UserId, long SenderAccountId)
{
    public override string ToString() => $"{UserId}:{SenderAccountId}";
}
