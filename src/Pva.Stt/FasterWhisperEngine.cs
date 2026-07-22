using System.IO;
using System.Text.Json;
using Pva.Core;

namespace Pva.Stt;

/// <summary>
/// موتور STT بر پایه‌ی <b>Faster Whisper</b> از طریق یک sidecar پایتون (engine pack
/// اختیاری). صدای هر قطعه در یک WAV موقت نوشته و مسیر آن به sidecar داده می‌شود تا از
/// انتقال آرایه‌های بزرگ روی JSON پرهیز شود.
///
/// پروتکل (خط JSON در هر جهت):
///   load:      {"cmd":"load","model":"&lt;dir&gt;","device":"cpu|gpu|auto"} → {"ok":true}
///   transcribe:{"cmd":"transcribe","wav":"&lt;path&gt;","language":"fa","prompt":"..."}
///                                   → {"ok":true,"text":"...","language":"fa"}
///
/// نیاز به python + مدل CTranslate2 دارد؛ تأیید نهایی دستی است. اگر در دسترس نباشد،
/// resolver به‌طور خودکار به whisper.cpp برمی‌گردد.
/// </summary>
public sealed class FasterWhisperEngine : ISpeechToTextEngine
{
    private readonly SttEngineOptions _options;
    private readonly ISidecarTransport _transport;
    private readonly bool _ownsTransport;
    private bool _loaded;

    public FasterWhisperEngine(SttEngineOptions options, ISidecarTransport? transport = null)
    {
        _options = options;

        if (transport is not null)
        {
            _transport = transport;
            _ownsTransport = false;
        }
        else
        {
            var python = options.PythonPath
                ?? throw new InvalidOperationException("مسیر پایتون (PythonPath) برای Faster Whisper تنظیم نشده است.");
            var script = options.SidecarScriptPath
                ?? throw new InvalidOperationException("مسیر اسکریپت sidecar تنظیم نشده است.");
            _transport = new ProcessSidecarTransport(python, script);
            _ownsTransport = true;
        }
    }

    public SpeechEngineKind Kind => SpeechEngineKind.FasterWhisper;

    public bool SupportsGpu => true;

    public bool IsLoaded => _loaded;

    public async Task LoadAsync(ModelConfig config, CancellationToken ct = default)
    {
        await _transport.StartAsync(ct);

        var model = _options.FasterWhisperModelDir ?? config.ModelPath;
        var request = JsonSerializer.Serialize(new
        {
            cmd = "load",
            model,
            device = config.Device.ToString().ToLowerInvariant(),
        });

        var response = await _transport.SendAsync(request, ct);
        if (!IsOk(response))
        {
            throw new InvalidOperationException($"بارگذاری مدل در sidecar ناموفق بود: {response}");
        }

        _loaded = true;
    }

    public async Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default)
    {
        if (!_loaded)
        {
            throw new InvalidOperationException("موتور بارگذاری نشده است؛ ابتدا LoadAsync را صدا بزنید.");
        }

        var wavPath = Path.Combine(Path.GetTempPath(), $"pva_{Guid.NewGuid():N}.wav");
        WavWriter.WriteMono(wavPath, audio.Samples.Span, audio.SampleRate);

        try
        {
            var request = JsonSerializer.Serialize(new
            {
                cmd = "transcribe",
                wav = wavPath,
                language = options.Language,
                prompt = options.InitialPrompt,
            });

            var response = await _transport.SendAsync(request, ct);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ok", out var ok) || !ok.GetBoolean())
            {
                throw new InvalidOperationException($"رونویسی در sidecar ناموفق بود: {response}");
            }

            var text = root.TryGetProperty("text", out var t) ? t.GetString() ?? string.Empty : string.Empty;
            var language = root.TryGetProperty("language", out var l) ? l.GetString() ?? options.Language : options.Language;
            return new SttResult(text.Trim(), language, null);
        }
        finally
        {
            TryDelete(wavPath);
        }
    }

    public void Unload() => _loaded = false;

    public async ValueTask DisposeAsync()
    {
        _loaded = false;
        if (_ownsTransport)
        {
            await _transport.DisposeAsync();
        }
    }

    private static bool IsOk(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            return doc.RootElement.TryGetProperty("ok", out var ok) && ok.GetBoolean();
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // فایل موقت؛ نادیده بگیر.
        }
    }
}
