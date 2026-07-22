namespace Pva.Core;

/// <summary>موتور تشخیص گفتار در دسترس.</summary>
public enum SpeechEngineKind
{
    /// <summary>whisper.cpp از طریق Whisper.net — پیش‌فرض پرتابل.</summary>
    WhisperCpp,

    /// <summary>Faster Whisper از طریق sidecar پایتون — engine pack اختیاری.</summary>
    FasterWhisper,
}

/// <summary>دستگاه محاسباتی برای اجرای مدل.</summary>
public enum ComputeDevice
{
    /// <summary>انتخاب خودکار: GPU در صورت وجود، وگرنه CPU.</summary>
    Auto,
    Cpu,
    Gpu,
}

/// <summary>پیکربندی بارگذاری مدل Whisper.</summary>
public sealed record ModelConfig
{
    /// <summary>مسیر فایل مدل (کنار exe در پوشه‌ی models/).</summary>
    public required string ModelPath { get; init; }

    public ComputeDevice Device { get; init; } = ComputeDevice.Auto;

    /// <summary>تعداد نخ‌های CPU؛ null یعنی انتخاب خودکار.</summary>
    public int? Threads { get; init; }
}

/// <summary>گزینه‌های یک رونویسی.</summary>
public sealed record SttOptions
{
    /// <summary>کد زبان؛ پیش‌فرض فارسی.</summary>
    public string Language { get; init; } = "fa";

    /// <summary>پرامپت اولیه برای جهت‌دهی تشخیص (اصطلاحات فارسی + فنی).</summary>
    public string? InitialPrompt { get; init; }
}

/// <summary>خروجی یک رونویسی.</summary>
public sealed record SttResult(string Text, string Language, double? Confidence);

/// <summary>
/// موتور تبدیل گفتار به متن. هر دو backend (whisper.cpp و Faster Whisper) این
/// قرارداد را پیاده می‌کنند. پیاده‌سازی در Milestone M2 (Pva.Stt).
/// </summary>
public interface ISpeechToTextEngine : IAsyncDisposable
{
    SpeechEngineKind Kind { get; }

    bool SupportsGpu { get; }

    bool IsLoaded { get; }

    Task LoadAsync(ModelConfig config, CancellationToken ct = default);

    Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default);

    void Unload();
}
