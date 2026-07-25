using UzonMail.CorePlugin.Services.SendCore.Reading;
using UzonMail.CorePlugin.Services.SendCore.Runtime;

namespace UzonMailDotNET.Test.UzonMail.Core.SendCore.Runtime;

/// <summary>
/// 验证发送配额和分页读取配置的边界约束。
/// </summary>
[TestClass]
public sealed class SendCoreOptionsTests
{
    [TestMethod]
    public void ReaderOptions_RejectInvalidCapacities()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendItemReaderOptions { PageSize = 0 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendItemReaderOptions { PageSize = 10, MaxBufferedPerGroup = 9 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendItemReaderOptions { PageSize = 10, MaxBufferedGlobally = 9 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendItemReaderOptions { MaxActiveReaders = 0 }.Validate()
        );
    }

    [TestMethod]
    public void QuotaOptions_RejectInvalidLimitsAndLeaseDuration()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendingQuotaOptions { SystemHardLimit = 0 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendingQuotaOptions { OrganizationFairShare = 0 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendingQuotaOptions { UserFairShare = 0 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendingQuotaOptions { GroupFairShare = 0 }.Validate()
        );
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new SendingQuotaOptions { LeaseDuration = TimeSpan.Zero }.Validate()
        );
    }

    [TestMethod]
    public void DefaultOptions_AreValid()
    {
        new SendItemReaderOptions().Validate();
        new SendingQuotaOptions().Validate();
    }
}
