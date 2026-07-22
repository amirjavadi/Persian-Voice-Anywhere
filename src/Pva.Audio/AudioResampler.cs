namespace Pva.Audio;

/// <summary>
/// نمونه‌بردار خطیِ استریمی و <b>خالص</b> (mono) از نرخ ورودی به نرخ خروجی. حالت را بین
/// فراخوانی‌ها نگه می‌دارد تا بتوان صدا را تکه‌تکه (همان‌طور که از کارت صدا می‌رسد) تغذیه کرد.
/// درون‌یابی خطی برای VAD/دیکته کافی است؛ نیازی به کیفیت studio نیست.
/// </summary>
public sealed class AudioResampler
{
    private readonly double _step;      // نسبت نرخ ورودی به خروجی
    private readonly List<float> _buffer = new();
    private double _position;           // موقعیت اعشاری در _buffer

    public AudioResampler(int inputRate, int outputRate)
    {
        if (inputRate <= 0 || outputRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(inputRate), "نرخ نمونه‌برداری باید مثبت باشد.");
        }

        InputRate = inputRate;
        OutputRate = outputRate;
        _step = (double)inputRate / outputRate;
    }

    public int InputRate { get; }

    public int OutputRate { get; }

    /// <summary>یک تکه نمونه‌ی ورودی را می‌گیرد و نمونه‌های خروجی resample‌شده را برمی‌گرداند.</summary>
    public float[] Resample(ReadOnlySpan<float> input)
    {
        if (InputRate == OutputRate)
        {
            return input.ToArray();
        }

        _buffer.AddRange(input.ToArray());

        var output = new List<float>();
        while (_position + 1 < _buffer.Count)
        {
            var index = (int)_position;
            var frac = (float)(_position - index);
            output.Add((_buffer[index] * (1f - frac)) + (_buffer[index + 1] * frac));
            _position += _step;
        }

        // نمونه‌های کاملاً مصرف‌شده را از ابتدای بافر حذف کن تا حافظه پایین بماند.
        var consumed = (int)_position;
        if (consumed > 0)
        {
            _buffer.RemoveRange(0, consumed);
            _position -= consumed;
        }

        return output.ToArray();
    }

    /// <summary>بازنشانی حالت (شروع یک استریم جدید).</summary>
    public void Reset()
    {
        _buffer.Clear();
        _position = 0;
    }
}
