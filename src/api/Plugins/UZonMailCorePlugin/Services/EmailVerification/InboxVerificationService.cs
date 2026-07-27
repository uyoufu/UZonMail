using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailVerification;

/// <summary>
/// 编排收件箱验证并将结果应用到核心收件箱数据。
/// </summary>
public sealed class InboxVerificationService(
    SqlContext db,
    IServiceProvider serviceProvider,
    InboxFailureGroupService failureGroupService
) : IScopedService
{
    private const int VerificationConcurrency = 16;
    private const int VerificationPageSize = 500;

    /// <summary>
    /// 验证指定收件箱。
    /// </summary>
    public Task<InboxVerificationBatchSummary> VerifyInboxesAsync(
        long userId,
        IReadOnlyCollection<long> inboxIds,
        CancellationToken cancellationToken = default
    )
    {
        return VerifyQueryAsync(
            userId,
            db.Inboxes.Where(x => x.UserId == userId && inboxIds.Contains(x.Id)),
            cancellationToken
        );
    }

    /// <summary>
    /// 验证指定分类中的全部收件箱。
    /// </summary>
    public Task<InboxVerificationBatchSummary> VerifyGroupAsync(
        long userId,
        long groupId,
        CancellationToken cancellationToken = default
    )
    {
        return VerifyQueryAsync(
            userId,
            db.Inboxes.Where(x => x.UserId == userId && x.EmailGroupId == groupId),
            cancellationToken
        );
    }

    private async Task<InboxVerificationBatchSummary> VerifyQueryAsync(
        long userId,
        IQueryable<Inbox> inboxQuery,
        CancellationToken cancellationToken
    )
    {
        var totalCount = 0;
        var validCount = 0;
        var invalidCount = 0;
        var unknownCount = 0;
        var lastInboxId = 0L;

        while (true)
        {
            var inboxes = await inboxQuery
                .AsNoTracking()
                .Where(x => x.Id > lastInboxId)
                .OrderBy(x => x.Id)
                .Take(VerificationPageSize)
                .Select(x => new InboxVerificationRequest(x.UserId, x.Id, x.Email))
                .ToListAsync(cancellationToken);
            if (inboxes.Count == 0)
                break;

            lastInboxId = inboxes[^1].InboxId;
            var reports = new InboxVerificationReport[inboxes.Count];
            await Parallel.ForEachAsync(
                Enumerable.Range(0, inboxes.Count),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = VerificationConcurrency,
                    CancellationToken = cancellationToken,
                },
                async (index, token) =>
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var verifiers = scope.ServiceProvider.GetServices<IInboxVerifier>().ToList();
                    if (verifiers.Count == 0)
                        throw new KnownException("当前版本不支持收件箱验证");

                    var verifierReports = await Task.WhenAll(
                        verifiers.Select(x => x.VerifyAsync(inboxes[index], token))
                    );
                    reports[index] = MergeReports(inboxes[index], verifierReports);
                }
            );

            var invalidReasons = reports
                .Where(x => x.State == InboxVerificationState.Invalid)
                .ToDictionary(x => x.Request.InboxId, x => x.FailureReason);
            await failureGroupService.MarkInvalidAsync(userId, invalidReasons, cancellationToken);

            var nonInvalidReports = reports
                .Where(x => x.State != InboxVerificationState.Invalid)
                .ToList();
            var reportsByInboxId = nonInvalidReports.ToDictionary(x => x.Request.InboxId);
            var nonInvalidIds = nonInvalidReports.Select(x => x.Request.InboxId).ToList();
            var currentInboxes = await db
                .Inboxes.Where(x => x.UserId == userId && nonInvalidIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
            foreach (var inbox in currentInboxes)
            {
                var report = reportsByInboxId[inbox.Id];
                inbox.Status =
                    report.State == InboxVerificationState.Valid
                        ? InboxStatus.Valid
                        : InboxStatus.Unkown;
                inbox.ValidFailReason = report.FailureReason;
            }
            await db.SaveChangesAsync(cancellationToken);

            totalCount += reports.Length;
            validCount += reports.Count(x => x.State == InboxVerificationState.Valid);
            invalidCount += invalidReasons.Count;
            unknownCount += reports.Count(x => x.State == InboxVerificationState.Unknown);
        }

        return new InboxVerificationBatchSummary(
            totalCount,
            validCount,
            invalidCount,
            unknownCount
        );
    }

    private static InboxVerificationReport MergeReports(
        InboxVerificationRequest request,
        IReadOnlyList<InboxVerificationReport> reports
    )
    {
        var evidences = reports.SelectMany(x => x.Evidences).ToList();
        var invalidReport = reports.FirstOrDefault(x => x.State == InboxVerificationState.Invalid);
        if (invalidReport != null)
            return new InboxVerificationReport(
                request,
                InboxVerificationState.Invalid,
                evidences,
                invalidReport.FailureReason
            );

        if (reports.All(x => x.State == InboxVerificationState.Valid))
            return new InboxVerificationReport(
                request,
                InboxVerificationState.Valid,
                evidences,
                null
            );

        return new InboxVerificationReport(
            request,
            InboxVerificationState.Unknown,
            evidences,
            reports.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FailureReason))?.FailureReason
        );
    }
}
