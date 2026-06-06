using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.CorePlugin.Services.SendCore.Sender.MsGraph;
using UZonMail.CorePlugin.Services.SendCore.WaitList;

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

        private sealed class TestWeight(int weight, bool enable) : IWeight
        {
            public int Weight => weight;

            public bool Enable => enable;
        }
    }
}
