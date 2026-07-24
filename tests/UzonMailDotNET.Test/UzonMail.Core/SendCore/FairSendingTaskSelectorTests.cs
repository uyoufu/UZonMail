using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Runtime;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

[TestClass]
public sealed class FairSendingTaskSelectorTests
{
    private static readonly DateTime BaseDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Selection_MatchesReferenceOrderingAcrossGeneratedLoads()
    {
        var random = new Random(73421);
        for (var scenario = 0; scenario < 100; scenario++)
        {
            var organizationCount = random.Next(1, 7);
            var userCount = random.Next(organizationCount, 24);
            var users = Enumerable
                .Range(1, userCount)
                .Select(userId =>
                    (
                        UserId: (long)userId,
                        OrganizationId: (long)random.Next(1, organizationCount + 1)
                    )
                )
                .ToArray();

            var nextOutboxId = 1L;
            var candidates = users
                .SelectMany(user =>
                    Enumerable
                        .Range(0, random.Next(1, 7))
                        .Select(_ =>
                            Candidate(
                                user.OrganizationId,
                                user.UserId,
                                nextOutboxId++,
                                random.Next(0, 20)
                            )
                        )
                )
                .OrderBy(_ => random.Next())
                .ToArray();
            var workers = Enumerable
                .Range(0, random.Next(0, 25))
                .Select(_ => users[random.Next(users.Length)])
                .Select(user => new SendingWorkerAllocation(user.OrganizationId, user.UserId))
                .ToArray();
            var snapshot = new SendingDispatchSnapshot(
                candidates,
                workers,
                random.Next(0, 24),
                random.Next(1, 8),
                random.Next(1, 5)
            );

            CollectionAssert.AreEqual(
                SelectWithReference(snapshot),
                SelectWithCycle(snapshot),
                $"Scenario {scenario} produced a different selection order."
            );
        }
    }

    [TestMethod]
    public void Commit_UpdatesOrganizationAndUserFairness()
    {
        SendingTaskCandidate[] candidates =
        [
            Candidate(1, 10, 1, 0),
            Candidate(1, 11, 2, 1),
            Candidate(2, 20, 3, 2),
        ];
        var snapshot = new SendingDispatchSnapshot(candidates, [], 3, 1, 1);

        var selected = SelectWithCycle(snapshot);

        CollectionAssert.AreEqual(
            new[] { new OutboxKey(10, 1), new OutboxKey(20, 3), new OutboxKey(11, 2), },
            selected
        );
    }

    [TestMethod]
    public void Reject_RemovesCandidateWithoutIncreasingFairnessCounts()
    {
        SendingTaskCandidate[] candidates =
        [
            Candidate(1, 10, 1, 0),
            Candidate(2, 20, 2, 1),
            Candidate(1, 10, 3, 2),
        ];
        var cycle = new FairSendingTaskSelector().CreateCycle(
            new SendingDispatchSnapshot(candidates, [], 2, 4, 4)
        );

        Assert.IsTrue(cycle.TryReserveNext(out var rejected));
        Assert.AreEqual(new OutboxKey(10, 1), rejected.Key);
        cycle.Reject(rejected.Key);

        Assert.IsTrue(cycle.TryReserveNext(out var selected));
        Assert.AreEqual(new OutboxKey(20, 2), selected.Key);
        cycle.Commit(selected.Key);
    }

    [TestMethod]
    public void Selection_StopsAtConfiguredLimit()
    {
        var candidates = Enumerable.Range(1, 10).Select(id => Candidate(1, id, id, id)).ToArray();
        var snapshot = new SendingDispatchSnapshot(candidates, [], 3, 4, 2);

        var selected = SelectWithCycle(snapshot);

        Assert.HasCount(3, selected);
        Assert.HasCount(3, selected.Distinct());
    }

    [TestMethod]
    public void Selection_HandlesHighCandidateVolume()
    {
        const int candidateCount = 20_000;
        var candidates = Enumerable
            .Range(1, candidateCount)
            .Select(id => Candidate(id % 20, id % 400, id, id % 1_000))
            .ToArray();
        var workers = Enumerable
            .Range(0, 64)
            .Select(id => new SendingWorkerAllocation(id % 20, id % 400))
            .ToArray();
        var snapshot = new SendingDispatchSnapshot(candidates, workers, 64, 16, 4);

        var selected = SelectWithCycle(snapshot);

        Assert.HasCount(64, selected);
        Assert.HasCount(64, selected.Distinct());
    }

    private static SendingTaskCandidate Candidate(
        long organizationId,
        long userId,
        long outboxId,
        int createdOffset
    ) =>
        new(
            new OutboxKey(userId, outboxId),
            organizationId,
            userId,
            BaseDate.AddSeconds(createdOffset)
        );

    private static OutboxKey[] SelectWithCycle(SendingDispatchSnapshot snapshot)
    {
        var cycle = new FairSendingTaskSelector().CreateCycle(snapshot);
        var selected = new List<OutboxKey>();
        while (cycle.TryReserveNext(out var candidate))
        {
            selected.Add(candidate.Key);
            cycle.Commit(candidate.Key);
        }

        return [.. selected];
    }

    private static OutboxKey[] SelectWithReference(SendingDispatchSnapshot snapshot)
    {
        var remaining = snapshot.Candidates.ToList();
        var activeByOrganization = CountBy(snapshot.ActiveWorkers, worker => worker.OrganizationId);
        var activeByUser = CountBy(snapshot.ActiveWorkers, worker => worker.UserId);
        var selected = new List<OutboxKey>();

        while (selected.Count < snapshot.MaxSelections && remaining.Count > 0)
        {
            var candidate = remaining
                .OrderBy(item =>
                    GetCount(activeByOrganization, item.OrganizationId)
                    >= snapshot.OrganizationFairShare
                )
                .ThenBy(item => GetCount(activeByUser, item.UserId) >= snapshot.UserFairShare)
                .ThenBy(item => GetCount(activeByOrganization, item.OrganizationId))
                .ThenBy(item => GetCount(activeByUser, item.UserId))
                .ThenBy(item => item.CreateDate)
                .ThenBy(item => item.UserId)
                .ThenBy(item => item.Key.OutboxId)
                .First();

            remaining.Remove(candidate);
            selected.Add(candidate.Key);
            Increment(activeByOrganization, candidate.OrganizationId);
            Increment(activeByUser, candidate.UserId);
        }

        return [.. selected];
    }

    private static Dictionary<long, int> CountBy<T>(
        IEnumerable<T> values,
        Func<T, long> keySelector
    ) => values.GroupBy(keySelector).ToDictionary(group => group.Key, group => group.Count());

    private static int GetCount(IReadOnlyDictionary<long, int> counts, long key) =>
        counts.TryGetValue(key, out var count) ? count : 0;

    private static void Increment(Dictionary<long, int> counts, long key) =>
        counts[key] = GetCount(counts, key) + 1;
}
