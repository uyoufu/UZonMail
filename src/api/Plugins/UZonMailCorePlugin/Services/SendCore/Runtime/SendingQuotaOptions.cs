namespace UzonMail.CorePlugin.Services.SendCore.Runtime;

public sealed class SendingQuotaOptions
{
    public const string SectionName = "SendCore:Quota";

    public int SystemHardLimit { get; set; } = 64;

    public int OrganizationFairShare { get; set; } = 16;

    public int UserFairShare { get; set; } = 4;

    public int GroupFairShare { get; set; } = 2;

    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(5);

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(SystemHardLimit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(OrganizationFairShare, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(UserFairShare, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(GroupFairShare, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(LeaseDuration, TimeSpan.Zero);
    }
}
