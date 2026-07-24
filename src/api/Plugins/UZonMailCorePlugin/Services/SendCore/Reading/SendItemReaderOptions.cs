namespace UzonMail.CorePlugin.Services.SendCore.Reading;

public sealed class SendItemReaderOptions
{
    public const string SectionName = "SendCore:Reader";

    public int PageSize { get; set; } = 100;

    public int MaxBufferedPerGroup { get; set; } = 200;

    public int MaxBufferedGlobally { get; set; } = 50_000;

    public int MaxActiveReaders { get; set; } = 256;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(PageSize, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxBufferedPerGroup, PageSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxBufferedGlobally, PageSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxActiveReaders, 1);
    }
}
