using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using UzonMailDesktop.Configuration;

namespace UzonMailDesktop.Services;

internal sealed class PrerequisiteService : IPrerequisiteService, IDisposable
{
    internal const string DotNetRuntimeId = "dotnet-runtime";
    internal const string AspNetCoreRuntimeId = "aspnetcore-runtime";
    internal const string WebView2RuntimeId = "webview2-runtime";

    private readonly BackendOptions _backend;
    private readonly PrerequisiteOptions _options;
    private readonly ILogger<PrerequisiteService> _logger;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromMinutes(10) };

    public PrerequisiteService(
        IOptions<BackendOptions> backend,
        IOptions<PrerequisiteOptions> options,
        ILogger<PrerequisiteService> logger
    )
    {
        _backend = backend.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PrerequisiteItem>> DetectAsync(
        CancellationToken cancellationToken = default
    )
    {
        var requiredFrameworks = RuntimeConfigReader.Read(_backend);
        var installedFrameworks = await ReadInstalledFrameworksAsync(cancellationToken);
        var results = new List<PrerequisiteItem>();

        foreach (
            var required in requiredFrameworks.Where(x =>
                x.Name is "Microsoft.NETCore.App" or "Microsoft.AspNetCore.App"
            )
        )
        {
            var installed = installedFrameworks.Any(x =>
                x.Name == required.Name && IsCompatible(required.Version, x.Version)
            );
            var id =
                required.Name == "Microsoft.NETCore.App" ? DotNetRuntimeId : AspNetCoreRuntimeId;
            results.Add(
                new PrerequisiteItem(
                    id,
                    $"{required.Name} {required.Version.Major}.{required.Version.Minor}",
                    installed,
                    required.Version
                )
            );
        }

        results.Add(
            new PrerequisiteItem(
                WebView2RuntimeId,
                "Microsoft Edge WebView2 Runtime",
                HasWebView2()
            )
        );
        return results;
    }

    public async Task InstallAsync(
        IEnumerable<PrerequisiteItem> prerequisites,
        IProgress<InstallProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var missing = prerequisites.Where(x => !x.IsInstalled).ToArray();
        var metadataCache = new Dictionary<string, JsonDocument>();
        var temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "UzonMailDesktop",
            "prerequisites"
        );
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            for (var index = 0; index < missing.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var item = missing[index];
                var basePercentage = 100d * index / missing.Length;
                var stepPercentage = 100d / missing.Length;

                if (item.Id == WebView2RuntimeId)
                {
                    var webViewInstallerPath = Path.Combine(
                        temporaryDirectory,
                        "MicrosoftEdgeWebView2Setup.exe"
                    );
                    await DownloadAsync(
                        _options.WebView2BootstrapperUrl,
                        webViewInstallerPath,
                        p =>
                            progress?.Report(
                                new InstallProgress(
                                    "正在下载 WebView2...",
                                    basePercentage + p * stepPercentage
                                )
                            ),
                        cancellationToken
                    );
                    ValidateMicrosoftSignature(webViewInstallerPath);
                    progress?.Report(
                        new InstallProgress(
                            "正在安装 WebView2...",
                            basePercentage + stepPercentage * 0.95
                        )
                    );
                    await RunInstallerAsync(
                        webViewInstallerPath,
                        cancellationToken,
                        "/silent",
                        "/install"
                    );
                    continue;
                }

                var requiredVersion =
                    item.RequiredVersion
                    ?? throw new InvalidOperationException($"{item.Name} 未声明所需版本。");
                var channel = $"{requiredVersion.Major}.{requiredVersion.Minor}";
                if (!metadataCache.TryGetValue(channel, out var metadata))
                {
                    var metadataUrl =
                        $"{_options.DotNetReleaseMetadataBaseUrl.TrimEnd('/')}/{channel}/releases.json";
                    using var response = await _httpClient.GetAsync(metadataUrl, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    metadata = JsonDocument.Parse(
                        await response.Content.ReadAsStreamAsync(cancellationToken)
                    );
                    metadataCache[channel] = metadata;
                }

                var package = SelectInstaller(metadata.RootElement, item.Id);
                var installerPath = Path.Combine(temporaryDirectory, package.FileName);
                await DownloadAsync(
                    package.Url,
                    installerPath,
                    p =>
                        progress?.Report(
                            new InstallProgress(
                                $"正在下载 {item.Name}...",
                                basePercentage + p * stepPercentage
                            )
                        ),
                    cancellationToken
                );
                ValidateSha512(installerPath, package.Hash);
                progress?.Report(
                    new InstallProgress(
                        $"正在安装 {item.Name}...",
                        basePercentage + stepPercentage * 0.95
                    )
                );
                await RunInstallerAsync(
                    installerPath,
                    cancellationToken,
                    "/install",
                    "/quiet",
                    "/norestart"
                );
            }

            progress?.Report(new InstallProgress("依赖安装完成，正在重新检测...", 100));
        }
        finally
        {
            foreach (var metadata in metadataCache.Values)
                metadata.Dispose();
            TryDeleteDirectory(temporaryDirectory);
        }
    }

    internal static bool IsCompatible(Version required, Version installed) =>
        required.Major == installed.Major
        && required.Minor == installed.Minor
        && installed >= required;

    internal static IReadOnlyList<RequiredFramework> ParseRuntimeList(string output)
    {
        var result = new List<RequiredFramework>();
        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && Version.TryParse(parts[1], out var version))
                result.Add(new RequiredFramework(parts[0], version));
        }
        return result;
    }

    private async Task<IReadOnlyList<RequiredFramework>> ReadInstalledFrameworksAsync(
        CancellationToken cancellationToken
    )
    {
        var dotnetPath = ResolveDotNetPath();
        if (dotnetPath is null)
            return [];

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetPath,
                Arguments = "--list-runtimes",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode == 0 ? ParseRuntimeList(output) : [];
    }

    private static string? ResolveDotNetPath()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var installedPath = Path.Combine(programFiles, "dotnet", "dotnet.exe");
        if (File.Exists(installedPath))
            return installedPath;

        var pathEntries = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(
            Path.PathSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        return pathEntries.Select(x => Path.Combine(x, "dotnet.exe")).FirstOrDefault(File.Exists);
    }

    private static bool HasWebView2()
    {
        try
        {
            return !string.IsNullOrWhiteSpace(
                CoreWebView2Environment.GetAvailableBrowserVersionString()
            );
        }
        catch (WebView2RuntimeNotFoundException)
        {
            return false;
        }
        catch (COMException)
        {
            return false;
        }
    }

    internal static InstallerPackage SelectInstaller(JsonElement root, string prerequisiteId)
    {
        var releases = root.GetProperty("releases")
            .EnumerateArray()
            .Where(x => Version.TryParse(x.GetProperty("release-version").GetString(), out _))
            .OrderByDescending(x => Version.Parse(x.GetProperty("release-version").GetString()!));

        var componentName = prerequisiteId == DotNetRuntimeId ? "runtime" : "aspnetcore-runtime";
        foreach (var release in releases)
        {
            if (
                !release.TryGetProperty(componentName, out var component)
                || !component.TryGetProperty("files", out var files)
            )
                continue;

            foreach (var file in files.EnumerateArray())
            {
                if (file.GetProperty("rid").GetString() != "win-x64")
                    continue;
                var name = file.GetProperty("name").GetString();
                if (name is null || !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                return new InstallerPackage(
                    name,
                    file.GetProperty("url").GetString()!,
                    file.GetProperty("hash").GetString()!
                );
            }
        }

        throw new InvalidOperationException($"未在微软发布元数据中找到 {componentName} 的 win-x64 安装器。");
    }

    private async Task DownloadAsync(
        string url,
        string destination,
        Action<double> reportProgress,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Downloading prerequisite from {Url}", url);
        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
        var length = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = new FileStream(
            destination,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true
        );
        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            total += read;
            reportProgress(
                length is > 0 ? Math.Min(0.9, (double)total / length.Value * 0.9) : 0.45
            );
        }
    }

    private static void ValidateSha512(string filePath, string expectedHash)
    {
        using var stream = File.OpenRead(filePath);
        var actualHash = Convert.ToHexString(SHA512.HashData(stream));
        if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            throw new CryptographicException($"安装器校验失败：{Path.GetFileName(filePath)}");
    }

    private static void ValidateMicrosoftSignature(string filePath)
    {
        if (!AuthenticodeVerifier.IsTrusted(filePath))
            throw new CryptographicException("WebView2 安装器的 Authenticode 签名无效。");

#pragma warning disable SYSLIB0057
        var certificate = X509Certificate.CreateFromSignedFile(filePath);
        using var certificate2 = new X509Certificate2(certificate);
#pragma warning restore SYSLIB0057
        if (
            !certificate2.Subject.Contains(
                "Microsoft Corporation",
                StringComparison.OrdinalIgnoreCase
            )
        )
            throw new CryptographicException("WebView2 安装器不是由 Microsoft Corporation 签名的。");
    }

    private static async Task RunInstallerAsync(
        string filePath,
        CancellationToken cancellationToken,
        params string[] arguments
    )
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = ProcessWindowStyle.Hidden
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        try
        {
            using var process =
                Process.Start(startInfo) ?? throw new InvalidOperationException("安装器未能启动。");
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode is not 0 and not 3010)
                throw new InvalidOperationException($"安装器返回错误代码 {process.ExitCode}。");
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("用户取消了管理员授权。", exception, cancellationToken);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public void Dispose() => _httpClient.Dispose();

    internal sealed record InstallerPackage(string FileName, string Url, string Hash);

    private static class AuthenticodeVerifier
    {
        private static readonly Guid GenericVerifyV2 = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        public static bool IsTrusted(string filePath)
        {
            var fileInfo = new WinTrustFileInfo
            {
                Size = (uint)Marshal.SizeOf<WinTrustFileInfo>(),
                FilePath = Marshal.StringToCoTaskMemUni(filePath)
            };
            var fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFileInfo>());

            try
            {
                Marshal.StructureToPtr(fileInfo, fileInfoPointer, fDeleteOld: false);
                var trustData = new WinTrustData
                {
                    Size = (uint)Marshal.SizeOf<WinTrustData>(),
                    UiChoice = 2,
                    RevocationChecks = 0,
                    UnionChoice = 1,
                    FileInfo = fileInfoPointer,
                    StateAction = 0,
                    ProviderFlags = 0x00000100,
                    UiContext = 0
                };
                return WinVerifyTrust(IntPtr.Zero, GenericVerifyV2, ref trustData) == 0;
            }
            finally
            {
                Marshal.FreeHGlobal(fileInfoPointer);
                Marshal.FreeCoTaskMem(fileInfo.FilePath);
            }
        }

        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint WinVerifyTrust(
            IntPtr windowHandle,
            [MarshalAs(UnmanagedType.LPStruct)] Guid actionId,
            ref WinTrustData trustData
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WinTrustFileInfo
        {
            public uint Size;
            public IntPtr FilePath;
            public IntPtr FileHandle;
            public IntPtr KnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WinTrustData
        {
            public uint Size;
            public IntPtr PolicyCallbackData;
            public IntPtr SipClientData;
            public uint UiChoice;
            public uint RevocationChecks;
            public uint UnionChoice;
            public IntPtr FileInfo;
            public uint StateAction;
            public IntPtr StateData;
            public IntPtr UrlReference;
            public uint ProviderFlags;
            public uint UiContext;
            public IntPtr SignatureSettings;
        }
    }
}
