using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Controllers.MailConversations.DTOs;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.MailConversations;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.CorePlugin.Services.MailConversations;

/// <summary>
/// 管理用户级联系人标签，并保证联系人和标签不能跨用户关联。
/// </summary>
public sealed class MailTagService(SqlContext db) : IScopedService
{
    public async Task<List<MailTagDto>> GetAsync(
        long userId,
        CancellationToken cancellationToken = default
    ) =>
        await db
            .MailTags.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new MailTagDto(x.Id, x.Name, x.Color))
            .ToListAsync(cancellationToken);

    public async Task<MailTagDto> CreateAsync(
        long userId,
        long organizationId,
        UpsertMailTagRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var name = ValidateName(request.Name);
        var normalizedName = name.ToLowerInvariant();
        if (
            await db.MailTags.AnyAsync(
                x => x.UserId == userId && x.NormalizedName == normalizedName,
                cancellationToken
            )
        )
            throw new KnownException("标签名称已存在");
        var tag = new MailTag
        {
            UserId = userId,
            OrganizationId = organizationId,
            Name = name,
            Color = ValidateColor(request.Color),
        };
        db.MailTags.Add(tag);
        await db.SaveChangesAsync(cancellationToken);
        return new MailTagDto(tag.Id, tag.Name, tag.Color);
    }

    public async Task<MailTagDto> UpdateAsync(
        long userId,
        long tagId,
        UpsertMailTagRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var tag = await GetOwnedTagAsync(userId, tagId, cancellationToken);
        var name = ValidateName(request.Name);
        var normalizedName = name.ToLowerInvariant();
        if (
            await db.MailTags.AnyAsync(
                x => x.UserId == userId && x.Id != tagId && x.NormalizedName == normalizedName,
                cancellationToken
            )
        )
            throw new KnownException("标签名称已存在");
        tag.Name = name;
        tag.Color = ValidateColor(request.Color);
        await db.SaveChangesAsync(cancellationToken);
        return new MailTagDto(tag.Id, tag.Name, tag.Color);
    }

    public async Task DeleteAsync(
        long userId,
        long tagId,
        CancellationToken cancellationToken = default
    )
    {
        var tag = await GetOwnedTagAsync(userId, tagId, cancellationToken);
        var relations = await db
            .MailContactTags.Where(x => x.MailTagId == tagId)
            .ToListAsync(cancellationToken);
        db.MailContactTags.RemoveRange(relations);
        db.MailTags.Remove(tag);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetContactTagsAsync(
        long userId,
        long contactId,
        IReadOnlyCollection<long> tagIds,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await db.MailContacts.AnyAsync(
                x => x.Id == contactId && x.UserId == userId,
                cancellationToken
            )
        )
            throw new KnownException("邮件联系人不存在");
        var distinctTagIds = tagIds.Where(x => x > 0).Distinct().ToList();
        var ownedTagCount = await db.MailTags.CountAsync(
            x => x.UserId == userId && distinctTagIds.Contains(x.Id),
            cancellationToken
        );
        if (ownedTagCount != distinctTagIds.Count)
            throw new KnownException("包含无效的联系人标签");
        var existing = await db
            .MailContactTags.Where(x => x.MailContactId == contactId)
            .ToListAsync(cancellationToken);
        db.MailContactTags.RemoveRange(existing.Where(x => !distinctTagIds.Contains(x.MailTagId)));
        var existingIds = existing.Select(x => x.MailTagId).ToHashSet();
        db.MailContactTags.AddRange(
            distinctTagIds
                .Where(x => !existingIds.Contains(x))
                .Select(x => new MailContactTag { MailContactId = contactId, MailTagId = x })
        );
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<MailTag> GetOwnedTagAsync(
        long userId,
        long tagId,
        CancellationToken cancellationToken
    ) =>
        await db.MailTags.FirstOrDefaultAsync(
            x => x.Id == tagId && x.UserId == userId,
            cancellationToken
        ) ?? throw new KnownException("邮件标签不存在");

    private static string ValidateName(string name)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > 100)
            throw new KnownException("标签名称长度应为 1-100 个字符");
        return trimmedName;
    }

    private static string ValidateColor(string color)
    {
        var trimmedColor = color.Trim();
        if (trimmedColor.Length is < 4 or > 20 || !trimmedColor.StartsWith('#'))
            throw new KnownException("标签颜色格式无效");
        return trimmedColor;
    }
}
