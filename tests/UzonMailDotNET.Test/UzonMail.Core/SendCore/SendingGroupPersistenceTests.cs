using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.Notification.EmailNotifier;
using UzonMail.CorePlugin.Services.SendCore;
using UzonMail.CorePlugin.Services.SendCore.Utils;
using UzonMail.CorePlugin.SignalRHubs.SendEmail;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMailDotNET.Test.UzonMail.Core.SendCore.Support;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore;

/// <summary>
/// 验证发送组状态、统计和完成通知的持久化行为。
/// </summary>
[TestClass]
public sealed class SendingGroupPersistenceTests
{
    [TestMethod]
    [DataRow(SendingGroupStatus.Cancel, SendingItemStatus.Cancel)]
    [DataRow(SendingGroupStatus.Pause, SendingItemStatus.Failed)]
    [DataRow(SendingGroupStatus.Finish, SendingItemStatus.Success)]
    [DataRow(SendingGroupStatus.Sending, SendingItemStatus.Failed)]
    public void StatusMapper_ReturnsExpectedItemStatus(
        SendingGroupStatus groupStatus,
        SendingItemStatus expected
    ) => Assert.AreEqual(expected, SendingGroupStatusMapper.ToSendingItemStatus(groupStatus));

    [TestMethod]
    public async Task StatusService_EmptyIdsDoesNotChangeRows()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.Add(new SendingGroup { Id = 10, Status = SendingGroupStatus.Sending });
        await database.Db.SaveChangesAsync();

        await new SendingGroupStatusService(database.Db).UpdateSendingGroupStatus(
            [],
            SendingGroupStatus.Cancel,
            "cancelled"
        );

        Assert.AreEqual(
            SendingGroupStatus.Sending,
            (await database.Db.SendingGroups.SingleAsync()).Status
        );
    }

    [TestMethod]
    public async Task StatusService_UpdatesGroupsAndOnlyActiveItems()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.AddRange(
            new SendingGroup { Id = 10, Status = SendingGroupStatus.Sending },
            new SendingGroup { Id = 11, Status = SendingGroupStatus.Sending }
        );
        database.Db.SendingItems.AddRange(
            CreateItem(1, 10, SendingItemStatus.Pending),
            CreateItem(2, 10, SendingItemStatus.Sending),
            CreateItem(3, 10, SendingItemStatus.Success),
            CreateItem(4, 11, SendingItemStatus.Pending)
        );
        await database.Db.SaveChangesAsync();

        await new SendingGroupStatusService(database.Db).UpdateSendingGroupStatus(
            10,
            SendingGroupStatus.Cancel,
            "cancelled"
        );
        database.Db.ChangeTracker.Clear();

        var group = await database.Db.SendingGroups.FindAsync(10L);
        var items = await database.Db.SendingItems.OrderBy(x => x.Id).ToListAsync();
        Assert.AreEqual(SendingGroupStatus.Cancel, group!.Status);
        Assert.AreEqual(SendingItemStatus.Cancel, items[0].Status);
        Assert.AreEqual("cancelled", items[0].SendResult);
        Assert.AreEqual(SendingItemStatus.Cancel, items[1].Status);
        Assert.AreEqual(SendingItemStatus.Success, items[2].Status);
        Assert.AreEqual(SendingItemStatus.Pending, items[3].Status);
    }

    [TestMethod]
    public async Task GroupUpdater_RecalculatesSuccessAndTerminalCounts()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.Add(new SendingGroup { Id = 10 });
        database.Db.SendingItems.AddRange(
            CreateItem(1, 10, SendingItemStatus.Success),
            CreateItem(2, 10, SendingItemStatus.Read),
            CreateItem(3, 10, SendingItemStatus.Cancel),
            CreateItem(4, 10, SendingItemStatus.Failed),
            CreateItem(5, 10, SendingItemStatus.Pending)
        );
        await database.Db.SaveChangesAsync();

        var group = await SendingGroupUpdater.UpdateSendingGroupSentInfo(database.Db, 10);

        Assert.AreEqual(2, group.SuccessCount);
        Assert.AreEqual(4, group.SentCount);
    }

    [TestMethod]
    public async Task GroupFinisher_PersistsStatusAndSendsEndNotifications()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        database.Db.SendingGroups.Add(
            new SendingGroup
            {
                Id = 10,
                UserId = 20,
                Subjects = "subject",
                TotalCount = 3,
                SuccessCount = 2,
                Status = SendingGroupStatus.Sending,
            }
        );
        await database.Db.SaveChangesAsync();
        var hub = new RecordingHubContext();
        var notifier = new RecordingFinishedNotifier();
        var startDate = DateTime.UtcNow.AddMinutes(-1);

        await new SendingGroupFinisher(database.Db, hub, notifier).SetSendingGroupStatusAndNotify(
            10,
            SendingGroupStatus.Finish,
            startDate
        );

        var group = await database.Db.SendingGroups.FindAsync(10L);
        Assert.AreEqual(SendingGroupStatus.Finish, group!.Status);
        Assert.IsTrue(group.SendEndDate > startDate);
        Assert.AreSame(group, notifier.SendingGroup);
        Assert.HasCount(1, hub.Client.GroupProgressMessages);
        Assert.AreEqual(ProgressType.End, hub.Client.GroupProgressMessages[0].ProgressType);
        Assert.AreEqual(10L, hub.Client.GroupProgressMessages[0].SendingGroupId);
    }

    private static SendingItem CreateItem(long id, long groupId, SendingItemStatus status) =>
        new() { Id = id, SendingGroupId = groupId, UserId = 1, Status = status };

    private sealed class RecordingFinishedNotifier : ISendingGroupFinishedNotifier
    {
        internal SendingGroup? SendingGroup { get; private set; }

        public Task Notify(SendingGroup sendingGroup)
        {
            SendingGroup = sendingGroup;
            return Task.CompletedTask;
        }
    }
}
