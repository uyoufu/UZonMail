using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UzonMailDesktop.Configuration;

namespace UzonMailDesktop.Services;

internal sealed class BackendProcessManager : IBackendProcessManager
{
    private readonly BackendOptions _options;
    private readonly ILogger<BackendProcessManager> _logger;
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private Process? _process;

    public BackendProcessManager(
        IOptions<BackendOptions> options,
        ILogger<BackendProcessManager> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool OwnsProcess => _process is { HasExited: false };

    public async Task StartOrReuseAsync(CancellationToken cancellationToken = default)
    {
        if (await IsReadyAsync(cancellationToken))
        {
            _logger.LogInformation("Reusing backend at {Url}", _options.ReadinessUrl);
            return;
        }

        await StopOwnedProcessAsync();

        var executablePath = ResolvePath(_options.ExecutablePath);
        if (!File.Exists(executablePath))
            throw new FileNotFoundException("配置的后端程序不存在。", executablePath);

        var workingDirectory = string.IsNullOrWhiteSpace(_options.WorkingDirectory)
            ? Path.GetDirectoryName(executablePath)!
            : ResolvePath(_options.WorkingDirectory);
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException($"配置的后端工作目录不存在：{workingDirectory}");

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var argument in _options.Arguments)
            startInfo.ArgumentList.Add(argument);
        foreach (var pair in _options.EnvironmentVariables)
            startInfo.Environment[pair.Key] = pair.Value;

        _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        _process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                _logger.LogInformation("Backend: {Message}", e.Data);
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                _logger.LogWarning("Backend: {Message}", e.Data);
        };

        if (!_process.Start())
            throw new InvalidOperationException("后端程序未能启动。");
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var timeout = TimeSpan.FromSeconds(_options.StartupTimeoutSeconds);
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_process.HasExited)
                throw new InvalidOperationException($"后端程序已退出，退出码：{_process.ExitCode}。");
            if (await IsReadyAsync(cancellationToken))
                return;
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        await StopOwnedProcessAsync();
        throw new TimeoutException($"后端在 {timeout.TotalSeconds:0} 秒内未就绪。");
    }

    public async Task StopAsync()
    {
        if (_options.KeepAliveOnExit)
        {
            _process?.Dispose();
            _process = null;
            return;
        }

        await StopOwnedProcessAsync();
    }

    private async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                _options.ReadinessUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            return (int)response.StatusCode < 500;
        }
        catch (Exception exception)
            when (exception is HttpRequestException or TaskCanceledException)
        {
            return false;
        }
    }

    private async Task StopOwnedProcessAsync()
    {
        if (_process is null)
            return;

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        catch (InvalidOperationException) { }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }

    internal static string ResolvePath(string path) =>
        Path.GetFullPath(path, AppContext.BaseDirectory);

    public void Dispose()
    {
        _httpClient.Dispose();
        _process?.Dispose();
    }
}
