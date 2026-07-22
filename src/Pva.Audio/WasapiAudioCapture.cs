using System.Collections.Concurrent;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Pva.Core;

namespace Pva.Audio;

/// <summary>
/// ضبط صدا از میکروفون با WASAPI (NAudio). صدای دستگاه را به mono تبدیل، به 16kHz
/// resample، به فریم‌های VAD تقسیم و از طریق <see cref="SpeechSegmenter"/> به قطعه‌های
/// گفتاری تبدیل می‌کند. برای هر قطعه‌ی کامل، رویداد <see cref="SegmentReady"/> صادر می‌شود.
///
/// پردازش روی یک نخ پس‌زمینه انجام می‌شود؛ نخ UI هرگز بلاک نمی‌شود.
/// </summary>
public sealed class WasapiAudioCapture : IAudioCapture
{
    private readonly AudioCaptureOptions _options;
    private readonly IVoiceActivityDetector _vad;

    private WasapiCapture? _capture;
    private AudioResampler? _resampler;
    private SpeechSegmenter? _segmenter;
    private BlockingCollection<float[]>? _queue;
    private Thread? _worker;
    private readonly List<float> _frameAccumulator = new();

    public WasapiAudioCapture(AudioCaptureOptions options, IVoiceActivityDetector vad)
    {
        _options = options;
        _vad = vad;
    }

    public event EventHandler<AudioSegment>? SegmentReady;

    public bool IsCapturing { get; private set; }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (IsCapturing)
        {
            return Task.CompletedTask;
        }

        _vad.Reset();
        _segmenter = new SpeechSegmenter(_options);
        _frameAccumulator.Clear();
        _queue = new BlockingCollection<float[]>(new ConcurrentQueue<float[]>());

        _capture = new WasapiCapture();
        _resampler = new AudioResampler(_capture.WaveFormat.SampleRate, _options.SampleRate);
        _capture.DataAvailable += OnDataAvailable;

        _worker = new Thread(ProcessLoop) { IsBackground = true, Name = "pva-audio-vad" };
        _worker.Start();

        _capture.StartRecording();
        IsCapturing = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (!IsCapturing)
        {
            return Task.CompletedTask;
        }

        IsCapturing = false;

        if (_capture is not null)
        {
            _capture.DataAvailable -= OnDataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
            _capture = null;
        }

        _queue?.CompleteAdding();
        _worker?.Join(TimeSpan.FromSeconds(2));
        _worker = null;

        // قطعه‌ی نیمه‌تمام را ببند.
        var tail = _segmenter?.Flush();
        if (tail is not null)
        {
            SegmentReady?.Invoke(this, tail);
        }

        _queue?.Dispose();
        _queue = null;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(StopAsync());
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        var mono = ToMonoFloat(e.Buffer, e.BytesRecorded, _capture!.WaveFormat);
        if (mono.Length > 0)
        {
            _queue?.Add(mono);
        }
    }

    private void ProcessLoop()
    {
        var queue = _queue!;
        var resampler = _resampler!;
        var segmenter = _segmenter!;
        var frameSize = _options.FrameSize;

        foreach (var chunk in queue.GetConsumingEnumerable())
        {
            var resampled = resampler.Resample(chunk);
            _frameAccumulator.AddRange(resampled);

            while (_frameAccumulator.Count >= frameSize)
            {
                var frame = new float[frameSize];
                _frameAccumulator.CopyTo(0, frame, 0, frameSize);
                _frameAccumulator.RemoveRange(0, frameSize);

                var probability = _vad.Detect(frame);
                var segment = segmenter.Accept(frame, probability);
                if (segment is not null)
                {
                    SegmentReady?.Invoke(this, segment);
                }
            }
        }
    }

    /// <summary>بایت‌های خام دستگاه را به نمونه‌های float تک‌کاناله تبدیل می‌کند.</summary>
    private static float[] ToMonoFloat(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        var channels = format.Channels;
        float[] samples;

        if (format.Encoding == WaveFormatEncoding.IeeeFloat && format.BitsPerSample == 32)
        {
            var count = bytesRecorded / 4;
            samples = new float[count];
            Buffer.BlockCopy(buffer, 0, samples, 0, count * 4);
        }
        else if (format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample == 16)
        {
            var count = bytesRecorded / 2;
            samples = new float[count];
            for (var i = 0; i < count; i++)
            {
                var s = (short)(buffer[(i * 2) + 1] << 8 | buffer[i * 2]);
                samples[i] = s / 32768f;
            }
        }
        else
        {
            // فرمت پشتیبانی‌نشده: خالی برگردان (در اجرای واقعی، فرمت WASAPI معمولاً float32 است).
            return [];
        }

        if (channels <= 1)
        {
            return samples;
        }

        // downmix چندکاناله به mono با میانگین‌گیری.
        var monoCount = samples.Length / channels;
        var mono = new float[monoCount];
        for (var i = 0; i < monoCount; i++)
        {
            var sum = 0f;
            for (var c = 0; c < channels; c++)
            {
                sum += samples[(i * channels) + c];
            }

            mono[i] = sum / channels;
        }

        return mono;
    }
}
