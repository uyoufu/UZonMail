using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Common;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.Utils.Web.Exceptions;

namespace UzonMail.CorePlugin.Services.Emails;

/// <summary>
/// 邮箱分组的写入边界。账户数量由读取端聚合，不在分组实体中冗余保存。
/// </summary>
public sealed class EmailGroupService(SqlContext db, TokenService tokenService)
    : CurdService<EmailGroup>(db)
{
    public async Task<List<EmailGroup>> GetEmailGroupsAsync(
        long userId,
        EmailGroupCategory category,
        CancellationToken cancellationToken = default
    ) =>
        await Db
            .EmailGroups.Where(x => x.UserId == userId && x.Category == category)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// 获取当前用户的默认分组；首次访问时创建它。
    /// </summary>
    public async Task<EmailGroup> GetDefaultEmailGroup(
        EmailGroupCategory category = EmailGroupCategory.RecipientEmail
    )
    {
        var tokenPayloads = tokenService.GetTokenPayloads();
        if (tokenPayloads.Count == 0)
            throw new KnownException("无法获取用户信息");

        var defaultGroup = await Db.EmailGroups.FirstOrDefaultAsync(x =>
            x.IsDefault && x.UserId == tokenPayloads.UserId && x.Category == category
        );
        if (defaultGroup != null)
            return defaultGroup;

        defaultGroup = EmailGroup.GetDefaultEmailGroup(tokenPayloads.UserId, category);
        Db.EmailGroups.Add(defaultGroup);
        await Db.SaveChangesAsync();
        return defaultGroup;
    }

    public override async Task<EmailGroup> Create(EmailGroup emailGroup)
    {
        if (string.IsNullOrWhiteSpace(emailGroup.Name))
            throw new KnownException("组名不允许为空");
        if (
            await Db.EmailGroups.AnyAsync(x =>
                x.UserId == emailGroup.UserId
                && x.Category == emailGroup.Category
                && x.Name == emailGroup.Name
            )
        )
            throw new KnownException("组名重复");

        emailGroup.Name = emailGroup.Name.Trim();
        emailGroup.Order =
            await Db
                .EmailGroups.Where(x =>
                    x.UserId == emailGroup.UserId && x.Category == emailGroup.Category
                )
                .Select(x => (long?)x.Order)
                .MaxAsync() ?? -1;
        emailGroup.Order++;
        return await base.Create(emailGroup);
    }

    public async Task<EmailGroup> UpdateMetadataAsync(
        long userId,
        long emailGroupId,
        string name,
        string? description,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new KnownException("组名不允许为空");
        var group =
            await Db.EmailGroups.FirstOrDefaultAsync(
                x => x.Id == emailGroupId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮箱分组不存在");
        var normalizedName = name.Trim();
        if (
            await Db.EmailGroups.AnyAsync(
                x =>
                    x.Id != emailGroupId
                    && x.UserId == userId
                    && x.Category == group.Category
                    && x.Name == normalizedName,
                cancellationToken
            )
        )
            throw new KnownException("组名重复");

        group.Name = normalizedName;
        group.Description = description;
        await Db.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task ReorderAsync(
        long userId,
        EmailGroupCategory category,
        IReadOnlyCollection<long> emailGroupIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = emailGroupIds.Distinct().ToList();
        var groups = await Db
            .EmailGroups.Where(x => x.UserId == userId && x.Category == category)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        if (ids.Count != groups.Count || !groups.All(x => ids.Contains(x.Id)))
            throw new KnownException("分组排序请求不完整");

        for (var index = 0; index < ids.Count; index++)
            groups.Single(x => x.Id == ids[index]).Order = index;
        await Db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        long userId,
        long emailGroupId,
        CancellationToken cancellationToken = default
    )
    {
        var group =
            await Db.EmailGroups.FirstOrDefaultAsync(
                x => x.Id == emailGroupId && x.UserId == userId,
                cancellationToken
            ) ?? throw new KnownException("邮箱分组不存在");
        if (group.IsDefault)
            throw new KnownException("默认分组不允许删除");

        var isInUse =
            group.Category == EmailGroupCategory.EmailAccount
                ? await Db.EmailAccounts.AnyAsync(
                    x => x.EmailGroupId == group.Id,
                    cancellationToken
                )
                : await Db.RecipientContacts.AnyAsync(
                    x => x.EmailGroupId == group.Id,
                    cancellationToken
                );
        if (isInUse)
            throw new KnownException("分组内仍有账户，无法删除");

        group.IsDeleted = true;
        await Db.SaveChangesAsync(cancellationToken);
    }
}
