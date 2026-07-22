using Pva.Core;
using Pva.Stt;

namespace Pva.Tests;

/// <summary>
/// تست انتخاب/fallback موتور STT با موتورهای جعلی — مستقل از Whisper و مدل واقعی.
/// </summary>
public class HybridSpeechEngineResolverTests
{
    private sealed class FakeEngine(SpeechEngineKind kind, bool failOnLoad) : ISpeechToTextEngine
    {
        public SpeechEngineKind Kind => kind;
        public bool SupportsGpu => false;
        public bool IsLoaded { get; private set; }
        public bool Disposed { get; private set; }

        public Task LoadAsync(ModelConfig config, CancellationToken ct = default)
        {
            if (failOnLoad)
            {
                throw new InvalidOperationException("شبیه‌سازی نبودِ موتور/مدل.");
            }

            IsLoaded = true;
            return Task.CompletedTask;
        }

        public Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default)
            => Task.FromResult(new SttResult("متن", options.Language, null));

        public void Unload() => IsLoaded = false;

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    private static ModelConfig Cfg() => new() { ModelPath = "dummy" };

    private static SttCandidate Candidate(SpeechEngineKind kind, bool fail, out FakeEngine engine)
    {
        var e = new FakeEngine(kind, fail);
        engine = e;
        return new SttCandidate(kind, Cfg(), () => e);
    }

    [Fact]
    public async Task PreferredLoads_ReturnsPreferred()
    {
        var preferred = Candidate(SpeechEngineKind.FasterWhisper, fail: false, out _);
        var fallback = Candidate(SpeechEngineKind.WhisperCpp, fail: false, out _);
        var resolver = new HybridSpeechEngineResolver([preferred, fallback]);

        var engine = await resolver.ResolveAsync();

        Assert.Equal(SpeechEngineKind.FasterWhisper, engine.Kind);
        Assert.True(engine.IsLoaded);
    }

    [Fact]
    public async Task PreferredFails_FallsBackToWhisperCpp()
    {
        var preferred = Candidate(SpeechEngineKind.FasterWhisper, fail: true, out var preferredEngine);
        var fallback = Candidate(SpeechEngineKind.WhisperCpp, fail: false, out _);
        var resolver = new HybridSpeechEngineResolver([preferred, fallback]);

        var engine = await resolver.ResolveAsync();

        Assert.Equal(SpeechEngineKind.WhisperCpp, engine.Kind);
        Assert.True(engine.IsLoaded);
        Assert.True(preferredEngine.Disposed); // موتور شکست‌خورده dispose شده باشد
    }

    [Fact]
    public async Task AllFail_ThrowsInvalidOperation()
    {
        var a = Candidate(SpeechEngineKind.FasterWhisper, fail: true, out _);
        var b = Candidate(SpeechEngineKind.WhisperCpp, fail: true, out _);
        var resolver = new HybridSpeechEngineResolver([a, b]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync());
    }

    [Fact]
    public async Task SingleWorkingCandidate_IsReturned()
    {
        var only = Candidate(SpeechEngineKind.WhisperCpp, fail: false, out _);
        var resolver = new HybridSpeechEngineResolver([only]);

        var engine = await resolver.ResolveAsync();

        Assert.Equal(SpeechEngineKind.WhisperCpp, engine.Kind);
    }

    [Fact]
    public async Task PreferredFactoryThrows_FallsBack()
    {
        // شبیه‌سازی نبودِ engine pack: سازنده‌ی موتور ترجیحی استثنا می‌دهد.
        var preferred = new SttCandidate(
            SpeechEngineKind.FasterWhisper,
            Cfg(),
            () => throw new InvalidOperationException("engine pack نصب نیست."));
        var fallback = Candidate(SpeechEngineKind.WhisperCpp, fail: false, out _);
        var resolver = new HybridSpeechEngineResolver([preferred, fallback]);

        var engine = await resolver.ResolveAsync();

        Assert.Equal(SpeechEngineKind.WhisperCpp, engine.Kind);
    }

    [Fact]
    public void NoCandidates_Throws()
    {
        Assert.Throws<ArgumentException>(() => new HybridSpeechEngineResolver([]));
    }
}
