using System.Text;

namespace Pva.Core;

/// <summary>گزینه‌های خط‌لوله‌ی دیکته.</summary>
public sealed record DictationOptions
{
    public SttOptions Stt { get; init; } = new();

    public PersianTextOptions PersianText { get; init; } = new();

    public CommandOptions Commands { get; init; } = new();
}

/// <summary>
/// هماهنگ‌کننده‌ی خط‌لوله‌ی دیکته: ضبط صدا → STT → تفسیر دستور → پس‌پردازش فارسی →
/// تزریق متن. قطعه‌های صوتی را به‌ترتیب (سریالی) پردازش می‌کند و وضعیت را از طریق
/// <see cref="StateChanged"/> منتشر می‌سازد. فقط به اینترفیس‌های Core وابسته است، پس
/// با پیاده‌سازی‌های جعلی کاملاً unit-test می‌شود.
/// </summary>
public sealed class DictationOrchestrator : IAsyncDisposable
{
    private readonly IAudioCapture _audio;
    private readonly ISpeechToTextEngine _engine;
    private readonly ICommandParser _parser;
    private readonly IPersianTextProcessor _persian;
    private readonly ITextInjector _injector;
    private readonly DictationOptions _options;
    private readonly SemaphoreSlim _processing = new(1, 1);

    public DictationOrchestrator(
        IAudioCapture audio,
        ISpeechToTextEngine engine,
        ICommandParser parser,
        IPersianTextProcessor persian,
        ITextInjector injector,
        DictationOptions? options = null)
    {
        _audio = audio;
        _engine = engine;
        _parser = parser;
        _persian = persian;
        _injector = injector;
        _options = options ?? new DictationOptions();
    }

    /// <summary>هنگام تغییر وضعیت خط‌لوله صادر می‌شود.</summary>
    public event EventHandler<DictationState>? StateChanged;

    /// <summary>پس از تولید متن نهاییِ یک قطعه صادر می‌شود (برای تاریخچه/UI).</summary>
    public event EventHandler<string>? TranscriptionProduced;

    /// <summary>هنگام بروز خطا در پردازش یک قطعه صادر می‌شود (خط‌لوله متوقف نمی‌شود).</summary>
    public event EventHandler<Exception>? ProcessingFailed;

    public DictationState State { get; private set; } = DictationState.Idle;

    public bool IsRunning { get; private set; }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        _audio.SegmentReady += OnSegmentReady;
        SetState(DictationState.Listening);
        await _audio.StartAsync(ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        _audio.SegmentReady -= OnSegmentReady;
        await _audio.StopAsync(ct);
        SetState(DictationState.Idle);
    }

    /// <summary>پردازش یک قطعه‌ی صوتی (نمایان برای تست؛ در عمل از رویداد فراخوانی می‌شود).</summary>
    public async Task ProcessSegmentAsync(AudioSegment segment)
    {
        await _processing.WaitAsync();
        try
        {
            SetState(DictationState.Processing);
            var recognized = await _engine.TranscribeAsync(segment, _options.Stt);
            var parsed = _parser.Parse(recognized.Text, _options.Commands);

            SetState(DictationState.Injecting);
            var finalText = new StringBuilder();
            foreach (var part in parsed.Parts)
            {
                if (part.Action is { } action)
                {
                    await _injector.SendActionAsync(action);
                }
                else if (part.Text is { } text)
                {
                    var clean = _persian.Process(text, _options.PersianText);
                    await _injector.TypeAsync(clean);
                    finalText.Append(clean);
                }
            }

            TranscriptionProduced?.Invoke(this, finalText.ToString());
        }
        catch (Exception ex)
        {
            ProcessingFailed?.Invoke(this, ex);
        }
        finally
        {
            _processing.Release();
            SetState(IsRunning ? DictationState.Listening : DictationState.Idle);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _processing.Dispose();
    }

    private async void OnSegmentReady(object? sender, AudioSegment segment)
        => await ProcessSegmentAsync(segment);

    private void SetState(DictationState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, state);
    }
}
