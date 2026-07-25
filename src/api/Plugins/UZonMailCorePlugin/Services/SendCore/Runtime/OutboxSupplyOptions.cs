using UzonMail.Utils.Web.Configs;

namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

/// <summary>
/// 发件箱目录分页和自适应供给配置。
/// </summary>
[OptionName("SendCore:OutboxSupply")]
public sealed class OutboxSupplyOptions : IAppOptions
{
    public int CatalogPageSize { get; set; } = 256;

    public int ReadyTargetMultiplier { get; set; } = 4;

    public int ReadyLowWatermarkMultiplier { get; set; } = 2;

    public int MaxTrackedOutboxes { get; set; } = 100_000;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(CatalogPageSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(ReadyTargetMultiplier, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(ReadyLowWatermarkMultiplier, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            ReadyLowWatermarkMultiplier,
            ReadyTargetMultiplier
        );
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxTrackedOutboxes, CatalogPageSize);
    }
}
