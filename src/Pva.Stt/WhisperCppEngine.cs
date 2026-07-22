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

    public SpeechEngineKind Kind => SpeechEngineKind.WhisperCpp;

    /// <summary>GPU در بسته‌ی پایه فعال نیست؛ با نصب Whisper.net.Runtime.* مناسب فعال می‌شود (بعد از v1).</summary>
    public bool SupportsGpu => false;

    public bool IsLoaded => _factory is not null;

    public Task LoadAsync(ModelConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        // WhisperFactory.FromPath در صورت نبودِ فایل، استثنا می‌دهد که resolver آن را می‌گیرد.
        _factory = WhisperFactory.FromPath(config.ModelPath);
        return Task.CompletedTask;
    }

    public async Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default)
    {
        if (_factory is null)
        {
            throw new InvalidOperationException("موتور بارگذاری نشده است؛ ابتدا LoadAsync را صدا بزنید.");
        }

        var builder = _factory.CreateBuilder().WithLanguage(options.Language);
        if (!string.IsNullOrWhiteSpace(options.InitialPrompt))
        {
            builder = builder.WithPrompt(options.InitialPrompt);
        }

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
