using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Organization;

namespace UzonMail.DB.Managers.Cache;

/// <summary>
/// 用户组织归属的原始数据快照。
/// </summary>
public sealed record UserInfoSnapshot(long UserId, long DepartmentId, long OrganizationId)
{
    /// <summary>
    /// 从用户实体创建快照。
    /// </summary>
    public static UserInfoSnapshot FromUser(User user) =>
        new(user.Id, user.DepartmentId, user.OrganizationId);
}

/// <summary>
/// 用户信息派生缓存。
/// </summary>
public sealed class UserInfoCache : BaseDBCache<SqlContext, long>
{
    public long UserId => Args;

    public long DepartmentId { get; private set; }

    public long OrganizationId { get; private set; }

    /// <summary>
    /// 获取用户信息原始源键。
    /// </summary>
    public static CacheSourceKey<UserInfoSnapshot, long> GetSourceKey(long userId) => new(userId);

    /// <summary>
    /// 读取用户信息快照并登记为当前结果的依赖。
    /// </summary>
    public static Task<UserInfoSnapshot> GetSnapshotAsync(
        CacheBuildContext buildContext,
        SqlContext db,
        long userId,
        CancellationToken cancellationToken
    ) =>
        buildContext.GetSourceAsync(
            GetSourceKey(userId),
            async token =>
            {
                var user = await db
                    .Users.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == userId, token);
                return user is null
                    ? new UserInfoSnapshot(userId, 0, 0)
                    : UserInfoSnapshot.FromUser(user);
            },
            cancellationToken
        );

    /// <inheritdoc />
    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContext db,
        CancellationToken cancellationToken
    )
    {
        var userInfo = await GetSnapshotAsync(buildContext, db, UserId, cancellationToken);
        DepartmentId = userInfo.DepartmentId;
        OrganizationId = userInfo.OrganizationId;
    }
}
