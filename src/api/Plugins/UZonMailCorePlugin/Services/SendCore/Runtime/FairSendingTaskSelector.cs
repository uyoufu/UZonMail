using UzonMail.CorePlugin.Services.SendCore.Domain;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

internal readonly record struct SendingTaskCandidate(
    SenderAccountKey Key,
    long OrganizationId,
    long UserId,
    DateTime CreateDate
);

internal readonly record struct SendingWorkerAllocation(long OrganizationId, long UserId);

internal sealed record SendingDispatchSnapshot(
    IReadOnlyCollection<SendingTaskCandidate> Candidates,
    IReadOnlyCollection<SendingWorkerAllocation> ActiveWorkers,
    int MaxSelections,
    int OrganizationFairShare,
    int UserFairShare
);

internal sealed class FairSendingTaskSelector
{
    public FairSendingSelectionCycle CreateCycle(SendingDispatchSnapshot snapshot) => new(snapshot);
}

internal sealed class FairSendingSelectionCycle
{
    private readonly record struct CandidatePriority(
        DateTime CreateDate,
        long UserId,
        long SenderAccountId
    ) : IComparable<CandidatePriority>
    {
        public int CompareTo(CandidatePriority other)
        {
            var result = CreateDate.CompareTo(other.CreateDate);
            if (result != 0)
                return result;

            result = UserId.CompareTo(other.UserId);
            return result != 0 ? result : SenderAccountId.CompareTo(other.SenderAccountId);
        }
    }

    private readonly record struct SchedulingPriority(
        bool OrganizationAtFairShare,
        bool UserAtFairShare,
        int OrganizationActiveCount,
        int UserActiveCount,
        DateTime CreateDate,
        long UserId,
        long SenderAccountId
    ) : IComparable<SchedulingPriority>
    {
        public int CompareTo(SchedulingPriority other)
        {
            var result = OrganizationAtFairShare.CompareTo(other.OrganizationAtFairShare);
            if (result != 0)
                return result;

            result = UserAtFairShare.CompareTo(other.UserAtFairShare);
            if (result != 0)
                return result;

            result = OrganizationActiveCount.CompareTo(other.OrganizationActiveCount);
            if (result != 0)
                return result;

            result = UserActiveCount.CompareTo(other.UserActiveCount);
            if (result != 0)
                return result;

            result = CreateDate.CompareTo(other.CreateDate);
            if (result != 0)
                return result;

            result = UserId.CompareTo(other.UserId);
            return result != 0 ? result : SenderAccountId.CompareTo(other.SenderAccountId);
        }
    }

    private sealed class UserState(
        long organizationId,
        long userId,
        int activeCount,
        PriorityQueue<SendingTaskCandidate, CandidatePriority> candidates
    )
    {
        public long OrganizationId { get; } = organizationId;
        public long UserId { get; } = userId;
        public int ActiveCount { get; set; } = activeCount;
        public PriorityQueue<SendingTaskCandidate, CandidatePriority> Candidates { get; } =
            candidates;
    }

    private sealed class OrganizationState(
        long organizationId,
        int activeCount,
        PriorityQueue<UserState, SchedulingPriority> users
    )
    {
        public long OrganizationId { get; } = organizationId;
        public int ActiveCount { get; set; } = activeCount;
        public PriorityQueue<UserState, SchedulingPriority> Users { get; } = users;
    }

    private sealed record Reservation(
        OrganizationState Organization,
        UserState User,
        SendingTaskCandidate Candidate
    );

    private readonly int _organizationFairShare;
    private readonly int _userFairShare;
    private readonly PriorityQueue<OrganizationState, SchedulingPriority> _organizations;
    private int _remainingSelections;
    private Reservation? _reservation;

    public FairSendingSelectionCycle(SendingDispatchSnapshot snapshot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(snapshot.MaxSelections);
        ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.OrganizationFairShare, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(snapshot.UserFairShare, 1);

        _remainingSelections = snapshot.MaxSelections;
        _organizationFairShare = snapshot.OrganizationFairShare;
        _userFairShare = snapshot.UserFairShare;

        var activeByOrganization = CountBy(
            snapshot.ActiveWorkers,
            allocation => allocation.OrganizationId
        );
        var activeByUser = CountBy(snapshot.ActiveWorkers, allocation => allocation.UserId);

        // Only the oldest candidate can win within a user, and only the best user can win
        // within an organization. This keeps the original ordering with localized heap updates.
        var candidatesByUser = snapshot.Candidates.GroupBy(candidate =>
            (candidate.OrganizationId, candidate.UserId)
        );
        var usersByOrganization = new Dictionary<long, List<UserState>>();
        foreach (var candidates in candidatesByUser)
        {
            var candidateQueue = new PriorityQueue<SendingTaskCandidate, CandidatePriority>(
                candidates.Select(candidate =>
                    (
                        candidate,
                        new CandidatePriority(
                            candidate.CreateDate,
                            candidate.UserId,
                            candidate.Key.SenderAccountId
                        )
                    )
                )
            );
            var user = new UserState(
                candidates.Key.OrganizationId,
                candidates.Key.UserId,
                GetCount(activeByUser, candidates.Key.UserId),
                candidateQueue
            );
            if (!usersByOrganization.TryGetValue(user.OrganizationId, out var users))
            {
                users = [];
                usersByOrganization.Add(user.OrganizationId, users);
            }

            users.Add(user);
        }

        var organizations = new List<(OrganizationState, SchedulingPriority)>();
        foreach (var (organizationId, users) in usersByOrganization)
        {
            var userQueue = new PriorityQueue<UserState, SchedulingPriority>(
                users.Select(user => (user, CreateUserPriority(user)))
            );
            var organization = new OrganizationState(
                organizationId,
                GetCount(activeByOrganization, organizationId),
                userQueue
            );
            organizations.Add((organization, CreateOrganizationPriority(organization)));
        }

        _organizations = new PriorityQueue<OrganizationState, SchedulingPriority>(organizations);
    }

    public bool TryReserveNext(out SendingTaskCandidate candidate)
    {
        if (_reservation is not null)
            throw new InvalidOperationException("The current reservation must be completed first.");

        if (_remainingSelections == 0 || !_organizations.TryDequeue(out var organization, out _))
        {
            candidate = default;
            return false;
        }

        if (!organization.Users.TryDequeue(out var user, out _))
            throw new InvalidOperationException("The selected organization has no candidates.");
        if (!user.Candidates.TryDequeue(out candidate, out _))
            throw new InvalidOperationException("The selected user has no candidates.");

        _reservation = new Reservation(organization, user, candidate);
        return true;
    }

    public void Commit(SenderAccountKey key)
    {
        var reservation = TakeReservation(key);
        reservation.Organization.ActiveCount++;
        reservation.User.ActiveCount++;
        _remainingSelections--;
        Requeue(reservation.Organization, reservation.User);
    }

    public void Reject(SenderAccountKey key)
    {
        var reservation = TakeReservation(key);
        Requeue(reservation.Organization, reservation.User);
    }

    private Reservation TakeReservation(SenderAccountKey key)
    {
        var reservation = _reservation;
        if (reservation is null || reservation.Candidate.Key != key)
            throw new InvalidOperationException(
                "The candidate does not match the current reservation."
            );

        _reservation = null;
        return reservation;
    }

    private void Requeue(OrganizationState organization, UserState user)
    {
        if (user.Candidates.Count > 0)
            organization.Users.Enqueue(user, CreateUserPriority(user));
        if (organization.Users.Count > 0)
            _organizations.Enqueue(organization, CreateOrganizationPriority(organization));
    }

    private SchedulingPriority CreateUserPriority(UserState user)
    {
        var candidate = user.Candidates.Peek();
        return new SchedulingPriority(
            false,
            user.ActiveCount >= _userFairShare,
            0,
            user.ActiveCount,
            candidate.CreateDate,
            candidate.UserId,
            candidate.Key.SenderAccountId
        );
    }

    private SchedulingPriority CreateOrganizationPriority(OrganizationState organization)
    {
        var user = organization.Users.Peek();
        var candidate = user.Candidates.Peek();
        return new SchedulingPriority(
            organization.ActiveCount >= _organizationFairShare,
            user.ActiveCount >= _userFairShare,
            organization.ActiveCount,
            user.ActiveCount,
            candidate.CreateDate,
            candidate.UserId,
            candidate.Key.SenderAccountId
        );
    }

    private static Dictionary<long, int> CountBy<T>(
        IEnumerable<T> values,
        Func<T, long> keySelector
    )
    {
        var counts = new Dictionary<long, int>();
        foreach (var value in values)
        {
            var key = keySelector(value);
            counts[key] = GetCount(counts, key) + 1;
        }

        return counts;
    }

    private static int GetCount(IReadOnlyDictionary<long, int> counts, long key) =>
        counts.TryGetValue(key, out var count) ? count : 0;
}
