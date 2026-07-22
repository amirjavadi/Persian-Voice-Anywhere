using Pva.Audio;

namespace Pva.Tests;

/// <summary>تست‌های نمونه‌بردار خطی (resampler).</summary>
public class AudioResamplerTests
{
    [Fact]
    public void Downsample_48kTo16k_ProducesOneThirdSamples()
    {
        var resampler = new AudioResampler(48000, 16000);
        var input = new float[300];
        Array.Fill(input, 1f);

        var output = resampler.Resample(input);

        Assert.Equal(100, output.Length);
        Assert.All(output, v => Assert.Equal(1f, v, 3));
    }

    [Fact]
    public void SameRate_IsPassthrough()
    {
        var resampler = new AudioResampler(16000, 16000);
        var input = new[] { 0.1f, 0.2f, 0.3f, 0.4f };

        var output = resampler.Resample(input);

        Assert.Equal(input, output);
    }

    [Fact]
    public void Downsample_Ramp_StaysMonotonic()
    {
        var resampler = new AudioResampler(48000, 16000);
        var input = new float[300];
        for (var i = 0; i < input.Length; i++)
        {
            input[i] = i;
        }

        var output = resampler.Resample(input);

        Assert.NotEmpty(output);
        for (var i = 1; i < output.Length; i++)
        {
            Assert.True(output[i] >= output[i - 1], $"خروجی باید صعودی باشد در اندیس {i}");
        }
    }

    [Fact]
    public void Streaming_AcrossChunks_MatchesContinuousBoundary()
    {
        // تغذیه‌ی تکه‌تکه نباید نمونه‌ای گم کند: مجموع خروجی نزدیک نسبت نرخ‌هاست.
        var resampler = new AudioResampler(48000, 16000);
        var total = 0;
        for (var chunk = 0; chunk < 10; chunk++)
        {
            total += resampler.Resample(new float[480]).Length; // 480 ورودی ≈ 160 خروجی
        }

        // 10*480=4800 ورودی @48k → ~1600 خروجی @16k.
        Assert.InRange(total, 1595, 1600);
    }

    [Fact]
    public void InvalidRate_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AudioResampler(0, 16000));
    }
}
