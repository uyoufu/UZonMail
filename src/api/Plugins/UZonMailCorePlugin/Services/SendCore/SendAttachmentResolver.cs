using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.SendCore;

/// <summary>
/// 将数据库附件引用解析为发送时可读取的本地文件。
/// </summary>
public sealed class SendAttachmentResolver(SqlContext db) : IScopedService
{
    /// <summary>
    /// 解析发送项引用的有效本地附件。
    /// </summary>
    public async Task<IReadOnlyList<PreparedSendAttachment>> ResolveAsync(SendingItem sendingItem)
    {
        var fileUsageIds = sendingItem.Attachments?.Select(x => x.Id).ToList() ?? [];
        if (fileUsageIds.Count == 0)
            return [];

        var attachments = await db
            .FileUsages.Where(x => fileUsageIds.Contains(x.Id))
            .Include(x => x.FileObject)
            .ThenInclude(x => x.FileBucket)
            .Select(x => new
            {
                FullPath = $"{x.FileObject.FileBucket.RootDir}/{x.FileObject.Path}",
                FileName = x.DisplayName ?? x.FileName,
            })
            .ToListAsync();

        return
        [
            .. attachments
                .Where(x => File.Exists(x.FullPath))
                .Select(x => new PreparedSendAttachment(x.FileName, new FileInfo(x.FullPath))),
        ];
    }
}
