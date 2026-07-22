using Pva.Core;

namespace Pva.Stt;

/// <summary>تنظیمات موتور STT هیبرید (whisper.cpp پیش‌فرض + Faster Whisper اختیاری).</summary>
public sealed record SttEngineOptions
{
    /// <summary>موتور ترجیحی. اگر بارگذاری نشد و FallbackToWhisperCpp روشن باشد، به whisper.cpp برمی‌گردیم.</summary>
    public SpeechEngineKind Preferred { get; init; } = SpeechEngineKind.WhisperCpp;

    /// <summary>در صورت شکست موتور ترجیحی، به whisper.cpp (پرتابل و مطمئن) fallback شود.</summary>
    public bool FallbackToWhisperCpp { get; init; } = true;

    // --- whisper.cpp ---
    /// <summary>مسیر مدل ggml؛ null یعنی models/ggml-base.bin کنار فایل اجرایی.</summary>
    public string? WhisperModelPath { get; init; }

    public ComputeDevice Device { get; init; } = ComputeDevice.Auto;

    public int? Threads { get; init; }

    // --- Faster Whisper (engine pack اختیاری) ---
    /// <summary>پوشه‌ی مدل CTranslate2؛ null یعنی models/faster-whisper.</summary>
    public string? FasterWhisperModelDir { get; init; }

    /// <summary>مسیر مفسر پایتون (embeddable در engine pack).</summary>
    public string? PythonPath { get; init; }

    /// <summary>مسیر اسکریپت sidecar پایتون.</summary>
    public string? SidecarScriptPath { get; init; }
}
