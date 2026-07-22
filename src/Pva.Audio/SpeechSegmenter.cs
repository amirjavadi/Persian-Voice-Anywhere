using Pva.Core;

namespace Pva.Audio;

/// <summary>
/// ماشین حالتِ <b>خالص و قطعی</b> که از دنباله‌ی (فریم صوتی + احتمال گفتار) مرزهای یک
/// قطعه‌ی گفتاری را تعیین می‌کند. هیچ وابستگی به سخت‌افزار یا مدل ندارد؛ برای همین
/// کاملاً unit-testable است.
///
/// منطق:
/// - وقتی احتمال ≥ SpeechThreshold باشد، قطعه شروع می‌شود (به‌همراه pre-roll برای
///   جلوگیری از بریدگی ابتدای کلمه).
/// - در حین گفتار، فریم‌ها انباشته می‌شوند. اگر احتمال &lt; SilenceThreshold به‌مدت
///   ≥ MinSilenceMs ادامه یابد، قطعه بسته می‌شود.
/// - قطعه‌ای که گفتار مفید آن کمتر از MinSpeechMs باشد، دور ریخته می‌شود (نویز کوتاه).
/// - اگر طول قطعه از MaxSegmentMs بگذرد، به‌اجبار بسته و قطعه‌ی بعدی ادامه می‌یابد.
/// </summary>
public sealed class SpeechSegmenter
{
    private readonly AudioCaptureOptions _options;
    private readonly int _minSilenceSamples;
    private readonly int _minSpeechSamples;
    private readonly int _maxSegmentSamples;

    private readonly float[] _preRoll;
    private int _preHead;
    private int _preCount;

    private readonly List<float> _current = new();
    private bool _inSpeech;
    private int _silenceSamples;
    private int _preRollAdded;

    public SpeechSegmenter(AudioCaptureOptions options)
    {
        _options = options;
        var sr = options.SampleRate;
        _minSilenceSamples = options.MinSilenceMs * sr / 1000;
        _minSpeechSamples = options.MinSpeechMs * sr / 1000;
        _maxSegmentSamples = Math.Max(sr, options.MaxSegmentMs * sr / 1000);
        _preRoll = new float[Math.Max(0, options.PreRollMs * sr / 1000)];
    }

    /// <summary>آیا در حال حاضر داخل یک قطعه‌ی گفتاری هستیم.</summary>
    public bool InSpeech => _inSpeech;

    /// <summary>
    /// یک فریم و احتمال گفتار آن را می‌پذیرد. اگر قطعه‌ای در این فریم کامل شود آن را
    /// برمی‌گرداند، در غیر این‌صورت null.
    /// </summary>
    public AudioSegment? Accept(ReadOnlySpan<float> frame, float speechProbability)
    {
        if (!_inSpeech)
        {
            if (speechProbability >= _options.SpeechThreshold)
            {
                StartSegment(frame);
            }
            else
            {
                PushPreRoll(frame);
            }

            return null;
        }

        // داخل گفتار: فریم را انباشته کن.
        _current.AddRange(frame.ToArray());

        if (speechProbability < _options.SilenceThreshold)
        {
            _silenceSamples += frame.Length;
        }
        else
        {
            _silenceSamples = 0;
        }

        if (_silenceSamples >= _minSilenceSamples)
        {
            return CloseSegment();
        }

        if (_current.Count >= _maxSegmentSamples)
        {
            // قطعه بیش‌ازحد طولانی شد: به‌اجبار ببند و در همین گفتار ادامه بده.
            var forced = BuildSegment();
            _current.Clear();
            _silenceSamples = 0;
            _preRollAdded = 0;
            return forced;
        }

        return null;
    }

    /// <summary>در پایان ضبط، قطعه‌ی نیمه‌تمام را می‌بندد (در صورت وجود و معتبر بودن).</summary>
    public AudioSegment? Flush()
    {
        if (!_inSpeech)
        {
            return null;
        }

        return CloseSegment();
    }

    /// <summary>بازنشانی کامل حالت.</summary>
    public void Reset()
    {
        _current.Clear();
        _inSpeech = false;
        _silenceSamples = 0;
        _preRollAdded = 0;
        _preHead = 0;
        _preCount = 0;
    }

    private void StartSegment(ReadOnlySpan<float> frame)
    {
        _inSpeech = true;
        _silenceSamples = 0;
        _current.Clear();
        _preRollAdded = DrainPreRollInto(_current);
        _current.AddRange(frame.ToArray());
    }

    private AudioSegment? CloseSegment()
    {
        var segment = BuildSegment();
        _inSpeech = false;
        _silenceSamples = 0;
        _current.Clear();
        _preRollAdded = 0;
        _preHead = 0;
        _preCount = 0;
        return segment;
    }

    private AudioSegment? BuildSegment()
    {
        // گفتار مفید = کل نمونه‌ها منهای pre-roll و منهای سکوت انتهایی.
        var voiced = _current.Count - _preRollAdded - _silenceSamples;
        if (voiced < _minSpeechSamples)
        {
            return null;
        }

        var samples = _current.ToArray();
        var duration = TimeSpan.FromSeconds(samples.Length / (double)_options.SampleRate);
        return new AudioSegment(samples, _options.SampleRate, duration);
    }

    private void PushPreRoll(ReadOnlySpan<float> frame)
    {
        if (_preRoll.Length == 0)
        {
            return;
        }

        foreach (var sample in frame)
        {
            _preRoll[_preHead] = sample;
            _preHead = (_preHead + 1) % _preRoll.Length;
            if (_preCount < _preRoll.Length)
            {
                _preCount++;
            }
        }
    }

    private int DrainPreRollInto(List<float> destination)
    {
        if (_preRoll.Length == 0 || _preCount == 0)
        {
            return 0;
        }

        var start = (_preHead - _preCount + _preRoll.Length) % _preRoll.Length;
        for (var i = 0; i < _preCount; i++)
        {
            destination.Add(_preRoll[(start + i) % _preRoll.Length]);
        }

        var added = _preCount;
        _preHead = 0;
        _preCount = 0;
        return added;
    }
}
