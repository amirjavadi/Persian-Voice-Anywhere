using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Pva.Audio;

/// <summary>ثبت سرویس‌های ضبط صدا در DI.</summary>
public static class AudioServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IAudioCapture"/> (WASAPI) و <see cref="IVoiceActivityDetector"/> (Silero)
    /// را ثبت می‌کند. مدل Silero به‌صورت lazy فقط هنگام اولین resolve ساخته می‌شود، پس
    /// نبودِ فایل مدل، راه‌اندازی برنامه را نمی‌شکند.
    /// </summary>
    public static IServiceCollection AddAudioCapture(this IServiceCollection services, AudioCaptureOptions? options = null)
    {
        var resolved = options ?? new AudioCaptureOptions();
        services.AddSingleton(resolved);

        services.AddSingleton<IVoiceActivityDetector>(_ =>
        {
            var modelPath = resolved.VadModelPath
                ?? Path.Combine(AppContext.BaseDirectory, "models", "silero_vad.onnx");
            return new SileroVoiceActivityDetector(modelPath, resolved.SampleRate);
        });

        services.AddSingleton<Pva.Core.IAudioCapture, WasapiAudioCapture>();
        return services;
    }
}
