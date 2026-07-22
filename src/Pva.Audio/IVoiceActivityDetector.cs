namespace Pva.Audio;

/// <summary>
/// تشخیص فعالیت گفتاری (VAD): برای هر فریم صوتی، احتمال وجود گفتار را برمی‌گرداند.
/// از منطق قطعه‌بندی (<see cref="SpeechSegmenter"/>) جداست تا هر کدام مستقل تست/تعویض شوند.
/// </summary>
public interface IVoiceActivityDetector : IDisposable
{
    /// <summary>احتمال گفتار در بازه‌ی [0,1] برای یک فریم (طول = AudioCaptureOptions.FrameSize).</summary>
    float Detect(ReadOnlySpan<float> frame);

    /// <summary>بازنشانی حالت داخلی مدل (مثلاً در شروع یک ضبط جدید).</summary>
    void Reset();
}
