namespace Pva.Audio;

/// <summary>
/// تنظیمات ضبط صدا و قطعه‌بندی گفتار (VAD). مقادیر پیش‌فرض برای دیکته‌ی فارسی تنظیم شده‌اند.
/// </summary>
public sealed record AudioCaptureOptions
{
    /// <summary>نرخ نمونه‌برداری هدف که به موتور STT داده می‌شود (Whisper = 16kHz).</summary>
    public int SampleRate { get; init; } = 16000;

    /// <summary>اندازه‌ی هر فریم برای VAD؛ Silero v5 دقیقاً ۵۱۲ نمونه در 16kHz می‌خواهد.</summary>
    public int FrameSize { get; init; } = 512;

    /// <summary>آستانه‌ی شروع گفتار (احتمال ≥ این مقدار = گفتار).</summary>
    public float SpeechThreshold { get; init; } = 0.5f;

    /// <summary>آستانه‌ی سکوت (هیسترزیس؛ احتمال &lt; این مقدار = سکوت).</summary>
    public float SilenceThreshold { get; init; } = 0.35f;

    /// <summary>مدت سکوت لازم برای بستن یک قطعه (hangover).</summary>
    public int MinSilenceMs { get; init; } = 500;

    /// <summary>حداقل مدت گفتار برای اینکه یک قطعه معتبر باشد (فیلتر نویز کوتاه).</summary>
    public int MinSpeechMs { get; init; } = 250;

    /// <summary>حداکثر طول یک قطعه؛ فراتر از آن به‌اجبار بسته می‌شود.</summary>
    public int MaxSegmentMs { get; init; } = 30000;

    /// <summary>مقدار صدای پیش از شروع گفتار که برای جلوگیری از بریدگی ابتدای کلمه نگه داشته می‌شود.</summary>
    public int PreRollMs { get; init; } = 200;

    /// <summary>مسیر فایل مدل Silero VAD؛ null یعنی models/silero_vad.onnx کنار فایل اجرایی.</summary>
    public string? VadModelPath { get; init; }
}
