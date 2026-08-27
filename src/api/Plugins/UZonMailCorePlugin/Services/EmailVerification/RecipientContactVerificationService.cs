using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.EmailVerification;

/// <summary>
/// 编排收件箱验证并将结果应用到核心收件箱数据。
/// </summary>
public sealed class RecipientContactVerificationService(
    SqlContext db,
    IServiceProvider serviceProvider,
    RecipientContactFailureGroupService failureGroupService
) : IScopedService
{
    private const int VerificationConcurrency = 16;
    private const int VerificationPageSize = 500;

    /// <summary>
    /// 验证指定收件箱。
    /// </summary>
    public Task<RecipientContactVerificationBatchSummary> VerifyRecipientsAsync(
        long userId,
        IReadOnlyCollection<long> recipientContactIds,
        CancellationToken cancellationToken = default
    )
    {
        return VerifyQueryAsync(
            userId,
            db.RecipientContacts.Where(x =>
                x.UserId == userId && recipientContactIds.Contains(x.Id)
            ),
            cancellationToken
        );
    }

    /// <summary>
    /// 验证指定分类中的全部收件箱。
    /// </summary>
    public Task<RecipientContactVerificationBatchSummary> VerifyGroupAsync(
        long userId,
        long groupId,
        CancellationToken cancellationToken = default
    )
    {
        return VerifyQueryAsync(
            userId,
            db.RecipientContacts.Where(x => x.UserId == userId && x.EmailGroupId == groupId),
            cancellationToken
        );
    }

    private async Task<RecipientContactVerificationBatchSummary> VerifyQueryAsync(
        long userId,
        IQueryable<RecipientContact> recipientContactQuery,
        CancellationToken cancellationToken
    )
    {
        var totalCount = 0;
        var validCount = 0;
        var invalidCount = 0;
        var unknownCount = 0;
        var lastRecipientContactId = 0L;

        while (true)
        {
            var recipientContacts = await recipientContactQuery
                .AsNoTracking()
                .Where(x => x.Id > lastRecipientContactId)
                .OrderBy(x => x.Id)
                .Take(VerificationPageSize)
                .Select(x => new RecipientContactVerificationRequest(x.UserId, x.Id, x.Email))
                .ToListAsync(cancellationToken);
            if (recipientContacts.Count == 0)
                break;

            lastRecipientContactId = recipientContacts[^1].RecipientContactId;
            var reports = new RecipientContactVerificationReport[recipientContacts.Count];
            await Parallel.ForEachAsync(
                Enumerable.Range(0, recipientContacts.Count),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = VerificationConcurrency,
                    CancellationToken = cancellationToken,
                },
                async (index, token) =>
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var verifiers = scope
                        .ServiceProvider.GetServices<IRecipientContactVerifier>()
                        .ToList();
                    if (verifiers.Count == 0)
                        throw new KnownException("当前版本不支持收件箱验证");

                    var verifierReports = await Task.WhenAll(
                        verifiers.Select(x => x.VerifyAsync(recipientContacts[index], token))
                    );
                    reports[index] = MergeReports(recipientContacts[index], verifierReports);
                }
            );

            var invalidReasons = reports
                .Where(x => x.State == RecipientContactVerificationState.Invalid)
                .ToDictionary(x => x.Request.RecipientContactId, x => x.FailureReason);
            await failureGroupService.MarkInvalidAsync(userId, invalidReasons, cancellationToken);

            var nonInvalidReports = reports
                .Where(x => x.State != RecipientContactVerificationState.Invalid)
                .ToList();
            var reportsByRecipientContactId = nonInvalidReports.ToDictionary(x =>
                x.Request.RecipientContactId
            );
            var nonInvalidIds = nonInvalidReports
                .Select(x => x.Request.RecipientContactId)
                .ToList();
            var currentRecipients = await db
                .RecipientContacts.Where(x => x.UserId == userId && nonInvalidIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
            foreach (var recipientContact in currentRecipients)
            {
                var report = reportsByRecipientContactId[recipientContact.Id];
                recipientContact.ValidationStatus =
                    report.State == RecipientContactVerificationState.Valid
                        ? RecipientValidationStatus.Valid
                        : RecipientValidationStatus.Unknown;
                recipientContact.ValidationFailureReason = report.FailureReason;
            }
            await db.SaveChangesAsync(cancellationToken);

            totalCount += reports.Length;
            validCount += reports.Count(x => x.State == RecipientContactVerificationState.Valid);
            invalidCount += invalidReasons.Count;
            unknownCount += reports.Count(x =>
                x.State == RecipientContactVerificationState.Unknown
            );
        }

        return new RecipientContactVerificationBatchSummary(
            totalCount,
            validCount,
            invalidCount,
            unknownCount
        );
    }

    private static RecipientContactVerificationReport MergeReports(
        RecipientContactVerificationRequest request,
        IReadOnlyList<RecipientContactVerificationReport> reports
    )
    {
        var evidences = reports.SelectMany(x => x.Evidences).ToList();
        var invalidReport = reports.FirstOrDefault(x =>
            x.State == RecipientContactVerificationState.Invalid
        );
        if (invalidReport != null)
            return new RecipientContactVerificationReport(
                request,
                RecipientContactVerificationState.Invalid,
                evidences,
                invalidReport.FailureReason
            );

        if (reports.All(x => x.State == RecipientContactVerificationState.Valid))
            return new RecipientContactVerificationReport(
                request,
                RecipientContactVerificationState.Valid,
                evidences,
                null
            );

        return new RecipientContactVerificationReport(
            request,
            RecipientContactVerificationState.Unknown,
            evidences,
            reports.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.FailureReason))?.FailureReason
        );
    }
}
