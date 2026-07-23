using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ILogger<WasapiAudioCapture> _logger;

    private WasapiCapture? _capture;
    private AudioResampler? _resampler;
    private SpeechSegmenter? _segmenter;
    private BlockingCollection<float[]>? _queue;
    private Thread? _worker;
    private readonly List<float> _frameAccumulator = new();

    // شمارنده‌های تشخیصی (فقط برای لاگِ عیب‌یابی؛ بدون ذخیره‌ی محتوای صدا).
    private long _bufferCount;
    private long _sampleCount;
    private long _frameCount;
    private long _segmentCount;
    private float _maxProbability;

    public WasapiAudioCapture(
        AudioCaptureOptions options,
        IVoiceActivityDetector vad,
        ILogger<WasapiAudioCapture>? logger = null)
    {
        _options = options;
        _vad = vad;
        _logger = logger ?? NullLogger<WasapiAudioCapture>.Instance;
    }

    public event EventHandler<AudioSegment>? SegmentReady;

    public event EventHandler<Exception>? CaptureFailed;

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
        _bufferCount = _sampleCount = _frameCount = _segmentCount = 0;
        _maxProbability = 0f;
        _queue = new BlockingCollection<float[]>(new ConcurrentQueue<float[]>());

        _capture = new WasapiCapture();
        var format = _capture.WaveFormat;
        _resampler = new AudioResampler(format.SampleRate, _options.SampleRate);
        _capture.DataAvailable += OnDataAvailable;

        _logger.LogInformation(
            "شروع ضبط. دستگاه: {Encoding} {Bits}bit {Channels}ch @ {DeviceRate}Hz → هدف {TargetRate}Hz. تشخیص float: {IsFloat}",
            format.Encoding, format.BitsPerSample, format.Channels, format.SampleRate,
            _options.SampleRate, IsFloatFormat(format));

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
            _segmentCount++;
            SegmentReady?.Invoke(this, tail);
        }

        // خلاصه‌ی تشخیصی: اگر sampleCount صفر باشد یعنی صدا اصلاً از دستگاه نرسیده؛
        // اگر maxProb پایین باشد یعنی VAD گفتاری ندیده (میکروفون اشتباه/بی‌صدا).
        _logger.LogInformation(
            "پایان ضبط. بافرها: {Buffers}، نمونه‌ها: {Samples}، فریم‌های VAD: {Frames}، بیشینه احتمال گفتار: {MaxProb:F2}، قطعه‌ها: {Segments}",
            _bufferCount, _sampleCount, _frameCount, _maxProbability, _segmentCount);

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
        _bufferCount++;
        var mono = ToMonoFloat(e.Buffer, e.BytesRecorded, _capture!.WaveFormat);
        if (mono.Length > 0)
        {
            _sampleCount += mono.Length;
            _queue?.Add(mono);
        }
        else if (_bufferCount == 1)
        {
            // نخستین بافر خالی شد: فرمت دستگاه پشتیبانی نمی‌شود (بدترین حالت پیشین).
            _logger.LogWarning(
                "بافر صدا به‌صورت خالی رمزگشایی شد؛ فرمت دستگاه ({Encoding} {Bits}bit) پشتیبانی نمی‌شود.",
                _capture.WaveFormat.Encoding, _capture.WaveFormat.BitsPerSample);
        }
    }

    private void ProcessLoop()
    {
        var queue = _queue!;
        var resampler = _resampler!;
        var segmenter = _segmenter!;
        var frameSize = _options.FrameSize;

        // نخِ پس‌زمینه؛ هر استثنای هندل‌نشده (مثلاً خطای مدل VAD/ONNX) کل پروسه را می‌کشد.
        // پس آن را می‌گیریم و از طریق CaptureFailed گزارش می‌کنیم تا crashِ بی‌صدا رخ ندهد.
        try
        {
            foreach (var chunk in queue.GetConsumingEnumerable())
            {
                var resampled = resampler.Resample(chunk);
                _frameAccumulator.AddRange(resampled);

                while (_frameAccumulator.Count >= frameSize)
                {
                    var frame = new float[frameSize];
                    _frameAccumulator.CopyTo(0, frame, 0, frameSize);
                    _frameAccumulator.RemoveRange(0, frameSize);

                    _frameCount++;
                    var probability = _vad.Detect(frame);
                    if (probability > _maxProbability)
                    {
                        _maxProbability = probability;
                    }

                    var segment = segmenter.Accept(frame, probability);
                    if (segment is not null)
                    {
                        _segmentCount++;
                        _logger.LogInformation(
                            "قطعه‌ی گفتاری تولید شد: {Ms}ms",
                            (int)segment.Duration.TotalMilliseconds);
                        SegmentReady?.Invoke(this, segment);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطای مهلک در نخِ پردازش صدا.");
            CaptureFailed?.Invoke(this, ex);
        }
    }

    // زیرفرمت‌های WaveFormatExtensible (پرکاربردترین فرمتِ mix اشتراکی WASAPI روی ویندوز).
    private static readonly Guid SubTypeIeeeFloat = new("00000003-0000-0010-8000-00aa00389b71");

    /// <summary>بایت‌های خام دستگاه را به نمونه‌های float تک‌کاناله تبدیل می‌کند.</summary>
    private static float[] ToMonoFloat(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        var channels = format.Channels;
        var isFloat = IsFloatFormat(format);
        var bits = format.BitsPerSample;
        float[] samples;

        if (isFloat && bits == 32)
        {
            var count = bytesRecorded / 4;
            samples = new float[count];
            Buffer.BlockCopy(buffer, 0, samples, 0, count * 4);
        }
        else if (!isFloat && bits == 16)
        {
            var count = bytesRecorded / 2;
            samples = new float[count];
            for (var i = 0; i < count; i++)
            {
                var s = (short)(buffer[(i * 2) + 1] << 8 | buffer[i * 2]);
                samples[i] = s / 32768f;
            }
        }
        else if (!isFloat && bits == 32)
        {
            // PCM صحیحِ ۳۲ بیتی (برخی دستگاه‌ها).
            var count = bytesRecorded / 4;
            samples = new float[count];
            for (var i = 0; i < count; i++)
            {
                var s = BitConverter.ToInt32(buffer, i * 4);
                samples[i] = s / 2147483648f;
            }
        }
        else
        {
            // فرمت واقعاً پشتیبانی‌نشده (مثلاً ۲۴ بیتی packed): خالی برگردان.
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

    /// <summary>
    /// تشخیص اینکه نمونه‌ها IEEE float هستند یا PCM صحیح. فرمت mix اشتراکی WASAPI معمولاً
    /// به‌صورت <see cref="WaveFormatExtensible"/> (Encoding = Extensible) گزارش می‌شود، نه
    /// مستقیماً IeeeFloat؛ در آن حالت باید زیرفرمت (SubFormat) بررسی شود، وگرنه صدا بی‌صدا
    /// دور ریخته می‌شد و برنامه اصلاً ضبط نمی‌کرد.
    /// </summary>
    private static bool IsFloatFormat(WaveFormat format)
    {
        if (format.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            return true;
        }

        if (format.Encoding == WaveFormatEncoding.Pcm)
        {
            return false;
        }

        if (format is WaveFormatExtensible extensible)
        {
            return extensible.SubFormat == SubTypeIeeeFloat;
        }

        // فرمت ناشناخته: ۳۲ بیتی معمولاً float است (mix اشتراکی WASAPI).
        return format.BitsPerSample == 32;
    }
}
