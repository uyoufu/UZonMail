using System.ComponentModel.DataAnnotations;

namespace UzonMailDesktop.Configuration;

public sealed class BackendOptions
{
    public const string SectionName = "Backend";

    [Required]
    public string ExecutablePath { get; init; } = string.Empty;

    public string? WorkingDirectory { get; init; }

    public string[] Arguments { get; init; } = [];

    public Dictionary<string, string> EnvironmentVariables { get; init; } = [];

    [Required, Url]
    public string WebUrl { get; init; } = string.Empty;

    [Required, Url]
    public string ReadinessUrl { get; init; } = string.Empty;

    [Range(1, 600)]
    public int StartupTimeoutSeconds { get; init; } = 60;

    public bool KeepAliveOnExit { get; init; }
}
