using UzonMail.CorePlugin.Services.SendCore.Domain;
using UzonMail.CorePlugin.Services.SendCore.Reading;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Reading;

/// <summary>
/// 验证 EF 发送项分页源的筛选、游标和排序规则。
/// </summary>
[TestClass]
public sealed class EfSendItemPageSourceTests
{
    [TestMethod]
    public async Task ReadPageAsync_FiltersStatusesAndOrdersBySenderAccountThenId()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.AddRange(
            new SendingGroup { Id = 10, UserId = 1 },
            new SendingGroup { Id = 11, UserId = 1 }
        );
        database.Db.SendingItems.AddRange(
            CreateItem(5, 10, 2, SendingItemStatus.Created),
            CreateItem(2, 10, 1, SendingItemStatus.Failed),
            CreateItem(4, 10, 1, SendingItemStatus.Success),
            CreateItem(3, 10, 1, SendingItemStatus.Pending),
            CreateItem(6, 11, 1, SendingItemStatus.Created)
        );
        await database.Db.SaveChangesAsync();

        var results = await new EfSendItemPageSource(database.Db).ReadPageAsync(
            new SendItemPageRequest(10, SendItemCursor.Start, 10, null, false),
            CancellationToken.None
        );

        CollectionAssert.AreEqual(new long[] { 2, 5 }, results.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task ReadPageAsync_AppliesCursorSelectionPendingAndTake()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.Add(new SendingGroup { Id = 10, UserId = 1 });
        database.Db.SendingItems.AddRange(
            CreateItem(2, 10, 1, SendingItemStatus.Created),
            CreateItem(3, 10, 1, SendingItemStatus.Pending),
            CreateItem(4, 10, 2, SendingItemStatus.Failed),
            CreateItem(5, 10, 2, SendingItemStatus.Created)
        );
        await database.Db.SaveChangesAsync();

        var results = await new EfSendItemPageSource(database.Db).ReadPageAsync(
            new SendItemPageRequest(10, new SendItemCursor(1, 2), 2, [3, 4, 5], true),
            CancellationToken.None
        );

        CollectionAssert.AreEqual(new long[] { 3, 4 }, results.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public async Task ReadPageAsync_ExcludesHardBounceItemsFromAllCandidateStatuses()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.Add(new SendingGroup { Id = 10, UserId = 1 });
        database.Db.SendingItems.AddRange(
            CreateItem(2, 10, 1, SendingItemStatus.Failed),
            CreateItem(3, 10, 1, SendingItemStatus.Failed, isHardBounce: true),
            CreateItem(4, 10, 1, SendingItemStatus.Pending, isHardBounce: true)
        );
        await database.Db.SaveChangesAsync();

        var results = await new EfSendItemPageSource(database.Db).ReadPageAsync(
            new SendItemPageRequest(10, SendItemCursor.Start, 10, [2, 3, 4], true),
            CancellationToken.None
        );

        CollectionAssert.AreEqual(new long[] { 2 }, results.Select(x => x.Id).ToArray());
    }

    private static SendingItem CreateItem(
        long id,
        long groupId,
        long senderAccountId,
        SendingItemStatus status,
        bool isHardBounce = false
    ) =>
        new()
        {
            Id = id,
            SendingGroupId = groupId,
            UserId = 1,
            SenderAccountId = senderAccountId,
            Status = status,
            IsHardBounce = isHardBounce,
        };
}
