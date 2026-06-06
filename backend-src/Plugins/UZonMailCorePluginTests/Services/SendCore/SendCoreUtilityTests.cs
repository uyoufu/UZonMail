using UZonMail.CorePlugin.Services.Encrypt.Models;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Extensions;

namespace UZonMail.CorePluginTests.Services.SendCore
{
    [TestClass]
    public class SendCoreUtilityTests
    {
        [TestMethod]
        public void GetSendMailApiPath_ReturnsMe_ForPersonalAccount()
        {
            var result = MsGraphClient.GetSendMailApiPath("user@example.com", true);

            Assert.AreEqual("me", result);
        }

        [TestMethod]
        public void GetSendMailApiPath_ReturnsEncodedUserPath_ForOrganizationAccount()
        {
            var result = MsGraphClient.GetSendMailApiPath("user@example.com", false);

            Assert.AreEqual("users/user%40example.com", result);
        }

        [TestMethod]
        public void GetDataByWeight_DoesNotUseWeightTotalAsIndex()
        {
            var values = new Dictionary<int, TestWeight>
            {
                [1] = new(1, true),
                [2] = new(100, true)
            };

            for (var i = 0; i < 100; i++)
            {
                var result = values.GetDataByWeight<int, TestWeight>();
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void MatchSendingMeta_IncludesSpecificItemsInRecycleBin()
        {
            var metas = new SendingItemMetaList();
            metas.Add(new SendItemMeta(1, 7));

            var meta = metas.GetSendingMeta(7);

            Assert.IsNotNull(meta);
            Assert.IsTrue(metas.MatchSendingMeta(7, true));
        }

        [TestMethod]
        public void SendingGroupStatusMapper_MapsCancelToCancel()
        {
            var result = SendingGroupStatusMapper.ToSendingItemStatus(SendingGroupStatus.Cancel);

            Assert.AreEqual(SendingItemStatus.Cancel, result);
        }

        [TestMethod]
        public void SendingGroupStatusMapper_MapsPauseToFailed()
        {
            var result = SendingGroupStatusMapper.ToSendingItemStatus(SendingGroupStatus.Pause);

            Assert.AreEqual(SendingItemStatus.Failed, result);
        }

        [TestMethod]
        public void OutboxEmailAddress_GetSendingGroupIds_ReturnsDistinctGroupIds()
        {
            var outbox = CreateOutboxEmailAddress(
                OutboxEmailAddressType.Specific,
                sendingItemIds: [10, 11]
            );

            var groupIds = outbox.GetSendingGroupIds();

            CollectionAssert.AreEqual(new List<long> { 1 }, groupIds);
        }

        [TestMethod]
        public void OutboxEmailAddress_Update_MergesSharedAndSpecificTargets()
        {
            var specific = CreateOutboxEmailAddress(
                OutboxEmailAddressType.Specific,
                sendingItemIds: [10]
            );
            var shared = CreateOutboxEmailAddress(OutboxEmailAddressType.Shared);

            specific.Update(shared);
            specific.RemoveSepecificSendingItem(1, 10);

            Assert.IsTrue(specific.ContainsSendingGroup(1));
            Assert.AreEqual(OutboxEmailAddressType.Specific | OutboxEmailAddressType.Shared, specific.Type);
        }

        private static OutboxEmailAddress CreateOutboxEmailAddress(
            OutboxEmailAddressType type,
            List<long>? sendingItemIds = null
        )
        {
            var encryptParams = new EncryptParams();
            return new OutboxEmailAddress(
                new Outbox()
                {
                    Id = 7,
                    UserId = 3,
                    Email = "sender@example.com",
                    Name = "Sender",
                    Password = "password".AES(encryptParams.Key, encryptParams.Iv),
                    ReplyToEmails = "",
                    SmtpHost = "smtp.example.com",
                    SmtpPort = 465,
                    Weight = 1
                },
                1,
                encryptParams,
                type,
                sendingItemIds
            );
        }

        private sealed class TestWeight(int weight, bool enable) : IWeight
        {
            public int Weight => weight;

            public bool Enable => enable;
        }
    }
}
