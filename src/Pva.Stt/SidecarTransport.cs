using System.Diagnostics;
using System.IO;

namespace Pva.Stt;

/// <summary>
/// انتقال درخواست/پاسخ به sidecar فاستر ویسپر. هر درخواست یک خط JSON است و پاسخ هم یک
/// خط JSON. جدا از موتور تعریف شده تا موتور با یک transport جعلی هم قابل تست باشد.
/// </summary>
public interface ISidecarTransport : IAsyncDisposable
{
    bool IsRunning { get; }

    Task StartAsync(CancellationToken ct = default);

    Task<string> SendAsync(string jsonRequestLine, CancellationToken ct = default);
}

/// <summary>
/// transport مبتنی بر فرآیند: یک پروسه‌ی پایتون را اجرا و از طریق stdin/stdout با آن
/// خط‌به‌خط JSON رد و بدل می‌کند. نیاز به python + اسکریپت sidecar دارد (engine pack)؛
/// تأیید نهایی دستی است.
/// </summary>
public sealed class ProcessSidecarTransport : ISidecarTransport
{
    private readonly string _pythonPath;
    private readonly string _scriptPath;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private Process? _process;

    public ProcessSidecarTransport(string pythonPath, string scriptPath)
    {
        _pythonPath = pythonPath;
        _scriptPath = scriptPath;
    }

    public bool IsRunning => _process is { HasExited: false };

    public Task StartAsync(CancellationToken ct = default)
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _pythonPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add(_scriptPath);

        _process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"اجرای sidecar پایتون ناموفق بود: {_pythonPath} {_scriptPath}");

        return Task.CompletedTask;
    }

    public async Task<string> SendAsync(string jsonRequestLine, CancellationToken ct = default)
    {
        if (_process is null || _process.HasExited)
        {
            throw new InvalidOperationException("sidecar در حال اجرا نیست.");
        }

        await _mutex.WaitAsync(ct);
        try
        {
            await _process.StandardInput.WriteLineAsync(jsonRequestLine.AsMemory(), ct);
            await _process.StandardInput.FlushAsync(ct);

            var response = await _process.StandardOutput.ReadLineAsync(ct)
                ?? throw new InvalidOperationException("sidecar پاسخی نداد (stdout بسته شد).");
            return response;
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _mutex.Dispose();

        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.StandardInput.Close();
                if (!_process.WaitForExit(1500))
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
        }
        catch
        {
            // در حال خاموش‌شدن؛ خطاها را نادیده بگیر.
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }

        await Task.CompletedTask;
    }
}
