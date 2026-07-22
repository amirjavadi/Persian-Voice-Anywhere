using System.IO;

namespace Pva.Stt;

/// <summary>نویسنده‌ی سبک فایل WAV (PCM 16-bit، mono) برای تبادل با sidecar فاستر ویسپر.</summary>
internal static class WavWriter
{
    public static void WriteMono(string path, ReadOnlySpan<float> samples, int sampleRate)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(stream);

        var dataBytes = samples.Length * 2;
        var byteRate = sampleRate * 2; // mono، 16-bit

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16);              // اندازه‌ی chunk
        writer.Write((short)1);        // PCM
        writer.Write((short)1);        // mono
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)2);        // block align
        writer.Write((short)16);       // bits per sample

        // data chunk
        writer.Write("data"u8);
        writer.Write(dataBytes);
        foreach (var sample in samples)
        {
            var clamped = Math.Clamp(sample, -1f, 1f);
            writer.Write((short)(clamped * short.MaxValue));
        }
    }
}
