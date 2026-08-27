namespace UzonMail.CorePlugin.Services.SendCore.Domain;

public enum SendLeaseState
{
    Active,
    Completed,
    Revoked,
    Expired,
}

public sealed record SendLease(
    Guid LeaseId,
    SendItemDescriptor Item,
    SenderAccountKey SenderAccount,
    DateTimeOffset AcquiredAt,
    DateTimeOffset ExpiresAt,
    SendLeaseState State = SendLeaseState.Active
);

public interface ISendLeaseStore
{
    bool TryAcquire(
        SendItemDescriptor item,
        SenderAccountKey senderAccount,
        DateTimeOffset now,
        TimeSpan duration,
        out SendLease lease
    );

    bool TryComplete(Guid leaseId, DateTimeOffset now, out SendLease? lease);

    bool TryRevoke(Guid leaseId, DateTimeOffset now, out SendLease? lease);

    IReadOnlyList<SendLease> ReclaimExpired(DateTimeOffset now);
}
