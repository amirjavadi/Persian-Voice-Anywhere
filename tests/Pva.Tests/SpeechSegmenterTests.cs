using Pva.Audio;
using Pva.Core;

namespace Pva.Tests;

/// <summary>
/// تست‌های ماشین حالت قطعه‌بندی گفتار. با VAD جعلی (احتمال‌های دستی) کار می‌کند تا
/// منطق مرزبندی مستقل از مدل و سخت‌افزار سنجیده شود.
/// </summary>
public class SpeechSegmenterTests
{
    // 16kHz، فریم 512 نمونه (۳۲ms). MinSpeech=250ms (۴۰۰۰ نمونه)، MinSilence=500ms (۸۰۰۰ نمونه).
    // PreRoll=0 برای شمارش دقیق در تست.
    private static AudioCaptureOptions Options() => new()
    {
        SampleRate = 16000,
        FrameSize = 512,
        SpeechThreshold = 0.5f,
        SilenceThreshold = 0.35f,
        MinSilenceMs = 500,
        MinSpeechMs = 250,
        MaxSegmentMs = 30000,
        PreRollMs = 0,
    };

    private static float[] Frame() => new float[512];

    private static List<AudioSegment> Feed(SpeechSegmenter segmenter, int frames, float probability)
    {
        var result = new List<AudioSegment>();
        for (var i = 0; i < frames; i++)
        {
            var segment = segmenter.Accept(Frame(), probability);
            if (segment is not null)
            {
                result.Add(segment);
            }
        }

        return result;
    }

    [Fact]
    public void Speech_ThenSilence_ClosesExactlyOneSegment()
    {
        var segmenter = new SpeechSegmenter(Options());

        var segments = new List<AudioSegment>();
        segments.AddRange(Feed(segmenter, frames: 20, probability: 0.9f)); // ~640ms speech
        segments.AddRange(Feed(segmenter, frames: 16, probability: 0.1f)); // 16*512=8192 ≥ 8000 → close

        Assert.Single(segments);
        // 20 speech + 16 silence frames، هر کدام 512 نمونه.
        Assert.Equal((20 + 16) * 512, segments[0].Samples.Length);
        Assert.Equal(16000, segments[0].SampleRate);
    }

    [Fact]
    public void ShortBlip_BelowMinSpeech_IsDiscarded()
    {
        var segmenter = new SpeechSegmenter(Options());

        var segments = new List<AudioSegment>();
        segments.AddRange(Feed(segmenter, frames: 3, probability: 0.9f)); // ~96ms < 250ms
        segments.AddRange(Feed(segmenter, frames: 16, probability: 0.1f)); // close attempt

        Assert.Empty(segments);
    }

    [Fact]
    public void Flush_EmitsOpenSegment()
    {
        var segmenter = new SpeechSegmenter(Options());
        Feed(segmenter, frames: 20, probability: 0.9f);

        var tail = segmenter.Flush();

        Assert.NotNull(tail);
        Assert.Equal(20 * 512, tail!.Samples.Length);
    }

    [Fact]
    public void Flush_WithNoSpeech_ReturnsNull()
    {
        var segmenter = new SpeechSegmenter(Options());
        Feed(segmenter, frames: 5, probability: 0.1f); // فقط سکوت

        Assert.Null(segmenter.Flush());
    }

    [Fact]
    public void TwoUtterances_SeparatedBySilence_ProduceTwoSegments()
    {
        var segmenter = new SpeechSegmenter(Options());
        var segments = new List<AudioSegment>();

        segments.AddRange(Feed(segmenter, frames: 20, probability: 0.9f));
        segments.AddRange(Feed(segmenter, frames: 16, probability: 0.1f)); // close #1
        segments.AddRange(Feed(segmenter, frames: 5, probability: 0.1f));  // gap
        segments.AddRange(Feed(segmenter, frames: 20, probability: 0.9f));
        segments.AddRange(Feed(segmenter, frames: 16, probability: 0.1f)); // close #2

        Assert.Equal(2, segments.Count);
    }

    [Fact]
    public void Hysteresis_BriefDipDoesNotCloseSegment()
    {
        var segmenter = new SpeechSegmenter(Options());
        var segments = new List<AudioSegment>();

        segments.AddRange(Feed(segmenter, frames: 10, probability: 0.9f));
        segments.AddRange(Feed(segmenter, frames: 4, probability: 0.1f));  // 4*512=2048 < 8000 → لحظه‌ای
        segments.AddRange(Feed(segmenter, frames: 10, probability: 0.9f)); // ادامه‌ی گفتار، سکوت ریست

        Assert.Empty(segments); // هنوز بسته نشده
        Assert.True(segmenter.InSpeech);
    }
}
