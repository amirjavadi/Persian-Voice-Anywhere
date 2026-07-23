using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pva.Core;

namespace Pva.Stt;

/// <summary>ثبت موتور STT هیبرید در DI.</summary>
public static class SttServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="ISpeechEngineResolver"/> را ثبت می‌کند که موتور ترجیحی را با fallback
    /// خودکار به whisper.cpp بارگذاری می‌کند. موتورها فقط هنگام ResolveAsync ساخته/بارگذاری
    /// می‌شوند، پس نبودِ مدل، راه‌اندازی برنامه را نمی‌شکند.
    /// </summary>
    public static IServiceCollection AddSpeechToText(this IServiceCollection services, SttEngineOptions? options = null)
    {
        var resolved = options ?? new SttEngineOptions();
        services.AddSingleton(resolved);
        services.AddSingleton<ISpeechEngineResolver>(sp =>
            new HybridSpeechEngineResolver(
                BuildCandidates(resolved),
                sp.GetService<ILogger<HybridSpeechEngineResolver>>()));

        return services;
    }

    /// <summary>
    /// ترتیب ترجیح مدل‌ها با GPU (Vulkan): turbo کیفیت عالی با سرعت قابل‌قبول دارد.
    /// (benchmark در docs/session.md سشن ۱۴.)
    /// </summary>
    private static readonly string[] PreferredModelsGpu =
    [
        "ggml-large-v3-turbo-q5_0.bin",
        "ggml-large-v3-turbo.bin",
        "ggml-medium-q5_0.bin",
        "ggml-small-q5_1.bin",
        "ggml-small.bin",
        "ggml-base.bin",
    ];

    /// <summary>
    /// ترتیب ترجیح در حالت Auto/CPU: مدل‌های بزرگ عمداً حذف شده‌اند — اگر Vulkan در
    /// دسترس نباشد fallback به CPU با ~۱۱x realtime دیکته را غیرقابل‌استفاده می‌کند.
    /// small-q5_1 روی هر دو خوب است (CPU ~1.1x، Vulkan ~0.4x).
    /// </summary>
    private static readonly string[] PreferredModelsSafe =
    [
        "ggml-small-q5_1.bin",
        "ggml-small.bin",
        "ggml-base.bin",
    ];

    private static string ResolveDefaultWhisperModel(ComputeDevice device)
    {
        var modelsDir = Path.Combine(AppContext.BaseDirectory, "models");
        // turbo فقط با انتخاب صریح GPU توسط کاربر (ADR-0013).
        var preferred = device == ComputeDevice.Gpu ? PreferredModelsGpu : PreferredModelsSafe;
        foreach (var name in preferred)
        {
            var candidate = Path.Combine(modelsDir, name);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // هیچ‌کدام نبود: مسیر پیش‌فرض (خطای واضحِ «مدل نیست» هنگام بارگذاری داده می‌شود).
        return Path.Combine(modelsDir, "ggml-base.bin");
    }

    private static List<SttCandidate> BuildCandidates(SttEngineOptions o)
    {
        var whisperPath = o.WhisperModelPath ?? ResolveDefaultWhisperModel(o.Device);
        var whisper = new SttCandidate(
            SpeechEngineKind.WhisperCpp,
            new ModelConfig { ModelPath = whisperPath, Device = o.Device, Threads = o.Threads },
            () => new WhisperCppEngine());

        var fasterDir = o.FasterWhisperModelDir
            ?? Path.Combine(AppContext.BaseDirectory, "models", "faster-whisper");
        var faster = new SttCandidate(
            SpeechEngineKind.FasterWhisper,
            new ModelConfig { ModelPath = fasterDir, Device = o.Device, Threads = o.Threads },
            () => new FasterWhisperEngine(o));

        var candidates = new List<SttCandidate>();
        if (o.Preferred == SpeechEngineKind.FasterWhisper)
        {
            candidates.Add(faster);
            if (o.FallbackToWhisperCpp)
            {
                candidates.Add(whisper);
            }
        }
        else
        {
            candidates.Add(whisper);
        }

        return candidates;
    }
}
