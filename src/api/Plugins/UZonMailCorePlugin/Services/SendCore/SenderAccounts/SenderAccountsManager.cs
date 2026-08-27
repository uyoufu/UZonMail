using System.Collections.Concurrent;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

public sealed class SenderAccountsManager : ISingletonService
{
    private readonly ConcurrentDictionary<SenderAccountKey, SenderEmailAddress> _senderAccounts =
    [];
    private readonly ConcurrentDictionary<
        long,
        ConcurrentDictionary<SenderAccountKey, byte>
    > _groupSenderAccounts = [];
    private readonly object _registrationLock = new();

    public IReadOnlyCollection<SenderEmailAddress> Values => [.. _senderAccounts.Values];

    public int Count => _senderAccounts.Count;

    public void AddSenderAccount(SenderEmailAddress senderAccount)
    {
        lock (_registrationLock)
        {
            var key = GetKey(senderAccount);
            var registeredSenderAccount = _senderAccounts.AddOrUpdate(
                key,
                senderAccount,
                (_, existing) =>
                {
                    existing.Update(senderAccount);
                    return existing;
                }
            );
            foreach (var groupId in registeredSenderAccount.GetSendingGroupIds())
            {
                _groupSenderAccounts.GetOrAdd(groupId, static _ => []).TryAdd(key, 0);
            }
        }
    }

    public bool RemoveSenderAccount(SenderEmailAddress senderAccount, string message)
    {
        lock (_registrationLock)
        {
            var key = GetKey(senderAccount);
            if (!_senderAccounts.TryRemove(key, out var removed))
                return false;
            foreach (var groupId in removed.GetSendingGroupIds())
            {
                if (_groupSenderAccounts.TryGetValue(groupId, out var groupSenderAccounts))
                    groupSenderAccounts.TryRemove(key, out _);
            }
            removed.MarkShouldDispose(message);
            return true;
        }
    }

    public List<SenderEmailAddress> RemoveSenderAccount(long sendingGroupId, string message)
    {
        lock (_registrationLock)
        {
            List<SenderEmailAddress> removedResults = [];
            if (!_groupSenderAccounts.TryRemove(sendingGroupId, out var linkedSenderAccounts))
                return removedResults;

            foreach (var key in linkedSenderAccounts.Keys)
            {
                if (!_senderAccounts.TryGetValue(key, out var senderAccount))
                    continue;
                senderAccount.RemoveSendingGroup(sendingGroupId);
                if (senderAccount.IsWorking || !_senderAccounts.TryRemove(key, out var removed))
                    continue;
                removed.MarkShouldDispose(message);
                removedResults.Add(removed);
            }
            return removedResults;
        }
    }

    public bool ExistValidSenderAccount(long sendingGroupId) =>
        _groupSenderAccounts.TryGetValue(sendingGroupId, out var linkedSenderAccounts)
        && linkedSenderAccounts.Keys.Any(key => ExistValidSenderAccount(key));

    /// <summary>
    /// 判断组内所有仍有效的发件箱是否都在等待每日额度重置。
    /// </summary>
    public bool AreAllSenderAccountsQuotaBlocked(long sendingGroupId, DateTimeOffset utcNow)
    {
        if (!_groupSenderAccounts.TryGetValue(sendingGroupId, out var linkedSenderAccounts))
            return false;

        var validSenderAccounts = linkedSenderAccounts
            .Keys.Select(key =>
                _senderAccounts.TryGetValue(key, out var senderAccount) ? senderAccount : null
            )
            .Where(senderAccount => senderAccount is { ShouldDispose: false })
            .ToList();
        return validSenderAccounts.Count > 0
            && validSenderAccounts.All(senderAccount => senderAccount!.IsQuotaBlocked(utcNow));
    }

    public bool ExistValidSenderAccount(SenderAccountKey key) =>
        _senderAccounts.TryGetValue(key, out var senderAccount) && !senderAccount.ShouldDispose;

    public bool ExistValidSenderAccount(string email) =>
        _senderAccounts.Values.Any(x => x.Email == email && !x.ShouldDispose);

    private static SenderAccountKey GetKey(SenderEmailAddress senderAccount) =>
        new(senderAccount.UserId, senderAccount.Id);
}
