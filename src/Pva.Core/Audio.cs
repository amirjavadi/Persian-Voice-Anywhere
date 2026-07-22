namespace Pva.Core;

/// <summary>
/// یک قطعه‌ی صوتی آماده برای رونویسی؛ PCM تک‌کاناله (mono) با نمونه‌های float در بازه‌ی [-1,1].
/// </summary>
public sealed record AudioSegment(ReadOnlyMemory<float> Samples, int SampleRate, TimeSpan Duration);

/// <summary>
/// ضبط صدا از میکروفون و قطعه‌بندی گفتار با VAD.
/// پیاده‌سازی در Milestone M1 (Pva.Audio).
/// </summary>
public interface IAudioCapture : IAsyncDisposable
{
    /// <summary>پس از تشخیص یک قطعه‌ی گفتاری کامل توسط VAD صادر می‌شود.</summary>
    event EventHandler<AudioSegment>? SegmentReady;

    bool IsCapturing { get; }

    Task StartAsync(CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);
}
