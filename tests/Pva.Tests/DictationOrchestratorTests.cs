using Pva.Core;
using Pva.PersianText;

namespace Pva.Tests;

/// <summary>تست خط‌لوله‌ی دیکته با پیاده‌سازی‌های جعلی + پس‌پردازش فارسی واقعی.</summary>
public class DictationOrchestratorTests
{
    private sealed class FakeAudioCapture : IAudioCapture
    {
        public event EventHandler<AudioSegment>? SegmentReady;
        public bool IsCapturing { get; private set; }
        public Task StartAsync(CancellationToken ct = default) { IsCapturing = true; return Task.CompletedTask; }
        public Task StopAsync(CancellationToken ct = default) { IsCapturing = false; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public void Emit(AudioSegment s) => SegmentReady?.Invoke(this, s);
    }

    private sealed class FakeEngine(string text) : ISpeechToTextEngine
    {
        public SpeechEngineKind Kind => SpeechEngineKind.WhisperCpp;
        public bool SupportsGpu => false;
        public bool IsLoaded => true;
        public Task LoadAsync(ModelConfig config, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SttResult> TranscribeAsync(AudioSegment audio, SttOptions options, CancellationToken ct = default)
            => Task.FromResult(new SttResult(text, "fa", null));
        public void Unload() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    // پارسر جعلی: متن را به یک بخش متنی و در صورت وجود «\n» یک کنش خط‌جدید تبدیل می‌کند.
    private sealed class FakeParser : ICommandParser
    {
        public ParsedTranscript Parse(string rawText, CommandOptions options)
        {
            var parts = new List<TranscriptPart>();
            var segments = rawText.Split('\n');
            for (var i = 0; i < segments.Length; i++)
            {
                if (segments[i].Length > 0)
                {
                    parts.Add(TranscriptPart.OfText(segments[i]));
                }

                if (i < segments.Length - 1)
                {
                    parts.Add(TranscriptPart.OfAction(EditorAction.NewLine));
                }
            }

            return new ParsedTranscript(parts);
        }
    }

    private sealed class RecordingInjector : ITextInjector
    {
        public List<string> Typed { get; } = new();
        public List<EditorAction> Actions { get; } = new();
        public Task TypeAsync(string text, CancellationToken ct = default) { Typed.Add(text); return Task.CompletedTask; }
        public Task SendActionAsync(EditorAction action, CancellationToken ct = default) { Actions.Add(action); return Task.CompletedTask; }
    }

    private static AudioSegment Segment() => new(new float[16000], 16000, TimeSpan.FromSeconds(1));

    private static DictationOrchestrator Build(string recognized, RecordingInjector injector)
        => new(new FakeAudioCapture(), new FakeEngine(recognized), new FakeParser(),
            new PersianTextProcessor(), injector);

    [Fact]
    public async Task Segment_IsTranscribed_PersianCleaned_AndInjected()
    {
        var injector = new RecordingInjector();
        var orchestrator = Build("می روم", injector);

        await orchestrator.ProcessSegmentAsync(Segment());

        Assert.Single(injector.Typed);
        Assert.Equal("می‌روم", injector.Typed[0]); // نیم‌فاصله اعمال شده
    }

    [Fact]
    public async Task ActionsInTranscript_AreSentAsKeys()
    {
        var injector = new RecordingInjector();
        var orchestrator = Build("خط اول\nخط دوم", injector);

        await orchestrator.ProcessSegmentAsync(Segment());

        Assert.Equal(2, injector.Typed.Count);
        Assert.Contains(EditorAction.NewLine, injector.Actions);
    }

    [Fact]
    public async Task TranscriptionProduced_IsRaised()
    {
        var injector = new RecordingInjector();
        var orchestrator = Build("سلام دنیا", injector);
        string? produced = null;
        orchestrator.TranscriptionProduced += (_, t) => produced = t;

        await orchestrator.ProcessSegmentAsync(Segment());

        Assert.Equal("سلام دنیا", produced);
    }

    [Fact]
    public async Task StartAndStop_TogglesStateAndCapture()
    {
        var audio = new FakeAudioCapture();
        var orchestrator = new DictationOrchestrator(audio, new FakeEngine("x"), new FakeParser(),
            new PersianTextProcessor(), new RecordingInjector());

        await orchestrator.StartAsync();
        Assert.True(orchestrator.IsRunning);
        Assert.True(audio.IsCapturing);
        Assert.Equal(DictationState.Listening, orchestrator.State);

        await orchestrator.StopAsync();
        Assert.False(orchestrator.IsRunning);
        Assert.False(audio.IsCapturing);
        Assert.Equal(DictationState.Idle, orchestrator.State);
    }
}
