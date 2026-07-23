using System.Text;
using Pva.Core;
using Whisper.net;

namespace Pva.Stt;

/// <summary>
/// موتور STT پیش‌فرض و پرتابل بر پایه‌ی <b>whisper.cpp</b> از طریق Whisper.net. کاملاً
/// native (بدون Python)، داخل ZIP، با ورودی 16kHz مونو float. مدل ggml از پوشه‌ی
/// models/ بارگذاری می‌شود.
/// </summary>
public sealed class WhisperCppEngine : ISpeechToTextEngine
{
    private WhisperFactory? _factory;
    private int _threads;

    public SpeechEngineKind Kind => SpeechEngineKind.WhisperCpp;

    /// <summary>شتاب GPU از طریق Vulkan (ADR-0013)؛ در نبودِ درایور/GPU خودکار به CPU برمی‌گردد.</summary>
    public bool SupportsGpu => true;

    public bool IsLoaded => _factory is not null;

    public Task LoadAsync(ModelConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        // ترتیب runtime بر اساس دستگاه: Vulkan (در Auto/Gpu) با fallback به CPU.
        // benchmark روی i7-1355U + Iris Xe: small-q5_1 با Vulkan ~0.4x realtime در برابر
        // ~1.1x روی CPU؛ turbo فقط با GPU قابل‌استفاده است (روی CPU: ~11x).
        Whisper.net.LibraryLoader.RuntimeOptions.RuntimeLibraryOrder = config.Device == ComputeDevice.Cpu
            ? [Whisper.net.LibraryLoader.RuntimeLibrary.Cpu]
            : [Whisper.net.LibraryLoader.RuntimeLibrary.Vulkan, Whisper.net.LibraryLoader.RuntimeLibrary.Cpu];

        // WhisperFactory.FromPath در صورت نبودِ فایل، استثنا می‌دهد که resolver آن را می‌گیرد.
        _factory = WhisperFactory.FromPath(config.ModelPath);
        // پیش‌فرض: همه‌ی هسته‌های فیزیکی منهای یکی (تا UI و ضبط نفس بکشند).
        _threads = config.Threads ?? Math.Max(2, Environment.ProcessorCount - 1);
        return Task.CompletedTask;
    }

    public async Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default)
    {
        if (_factory is null)
        {
            throw new InvalidOperationException("موتور بارگذاری نشده است؛ ابتدا LoadAsync را صدا بزنید.");
        }

        var builder = _factory.CreateBuilder()
            .WithLanguage(options.Language)
            .WithThreads(_threads);
        if (!string.IsNullOrWhiteSpace(options.InitialPrompt))
        {
            builder = builder.WithPrompt(options.InitialPrompt);
        }

        // beam search به‌جای greedy: دقت بالاتر روی فارسی به بهای کمی سرعت (ارزشش را دارد).
        builder.WithBeamSearchSamplingStrategy();

        await using var processor = builder.Build();

        var samples = audio.Samples.ToArray();
        var text = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(samples, ct))
        {
            text.Append(segment.Text);
        }

        return new SttResult(text.ToString().Trim(), options.Language, null);
    }

    public void Unload()
    {
        _factory?.Dispose();
        _factory = null;
    }

    public ValueTask DisposeAsync()
    {
        Unload();
        return ValueTask.CompletedTask;
    }
}
