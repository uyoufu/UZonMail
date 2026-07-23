using System.ComponentModel.DataAnnotations;

namespace UzonMailDesktop.Configuration;

public sealed class PrerequisiteOptions
{
    public const string SectionName = "Prerequisites";

    public bool ConfirmBeforeInstall { get; init; } = true;

    [Required, Url]
    public string DotNetReleaseMetadataBaseUrl { get; init; } = string.Empty;

    [Required, Url]
    public string WebView2BootstrapperUrl { get; init; } = string.Empty;
}
