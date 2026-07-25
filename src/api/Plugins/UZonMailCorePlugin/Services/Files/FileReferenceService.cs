using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.Files
{
    /// <summary>
    /// 维护发送项与逻辑文件之间的持久引用计数。
    /// </summary>
    public sealed class FileReferenceService(SqlContext db) : IScopedService
    {
        /// <summary>
        /// 按已保存发送项中的附件关系增加逻辑文件引用。
        /// </summary>
        public async Task IncreaseReferencesAsync(
            IReadOnlyCollection<SendingItem> sendingItems,
            CancellationToken cancellationToken = default
        )
        {
            var increments = sendingItems
                .SelectMany(x => x.Attachments ?? [])
                .GroupBy(x => x.Id)
                .Select(x => new { FileUsageId = x.Key, Count = (long)x.Count() })
                .ToList();
            foreach (var increment in increments)
            {
                await db
                    .FileUsages.Where(x => x.Id == increment.FileUsageId)
                    .ExecuteUpdateAsync(
                        setters =>
                            setters.SetProperty(
                                x => x.ReferenceCount,
                                x => x.ReferenceCount + increment.Count
                            ),
                        cancellationToken
                    );
            }
        }
    }
}
