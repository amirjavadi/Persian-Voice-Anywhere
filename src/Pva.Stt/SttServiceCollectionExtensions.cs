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

    private static List<SttCandidate> BuildCandidates(SttEngineOptions o)
    {
        var whisperPath = o.WhisperModelPath
            ?? Path.Combine(AppContext.BaseDirectory, "models", "ggml-base.bin");
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
