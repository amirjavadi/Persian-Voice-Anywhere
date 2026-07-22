using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Pva.Audio;

/// <summary>
/// پیاده‌سازی VAD با مدل <b>Silero VAD (نسخه‌ی ۵)</b> از طریق ONNX Runtime. مدل حالت
/// بازگشتی دارد؛ برای هر فریم ۵۱۲ نمونه‌ای (در 16kHz) یک احتمال گفتار و حالت جدید
/// برمی‌گرداند.
///
/// نکته: فایل مدل (silero_vad.onnx) جداگانه توزیع می‌شود و در پوشه‌ی models/ کنار
/// فایل اجرایی قرار می‌گیرد (git-ignore). این کلاس نیاز به فایل مدل و صدای واقعی دارد
/// و تأیید نهایی آن دستی/در اجرای واقعی است؛ منطق قطعه‌بندی جدا و unit-tested است.
/// </summary>
public sealed class SileroVoiceActivityDetector : IVoiceActivityDetector
{
    private const int StateLength = 2 * 1 * 128;

    private readonly InferenceSession _session;
    private readonly long _sampleRate;
    private float[] _state = new float[StateLength];

    public SileroVoiceActivityDetector(string modelPath, int sampleRate = 16000)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException(
                $"فایل مدل Silero VAD یافت نشد: {modelPath}. آن را در پوشه‌ی models/ کنار فایل اجرایی قرار دهید.",
                modelPath);
        }

        _session = new InferenceSession(modelPath);
        _sampleRate = sampleRate;
    }

    public float Detect(ReadOnlySpan<float> frame)
    {
        var input = new DenseTensor<float>(frame.ToArray(), [1, frame.Length]);
        var state = new DenseTensor<float>(_state, [2, 1, 128]);
        var sr = new DenseTensor<long>(new[] { _sampleRate }, Array.Empty<int>());

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", input),
            NamedOnnxValue.CreateFromTensor("state", state),
            NamedOnnxValue.CreateFromTensor("sr", sr),
        };

        using var results = _session.Run(inputs);

        // خروجی‌ها به‌ترتیب: [0] احتمال گفتار، [1] حالت جدید.
        var ordered = results.ToList();
        var probability = ordered[0].AsEnumerable<float>().First();
        _state = ordered[1].AsEnumerable<float>().ToArray();

        return probability;
    }

    public void Reset() => _state = new float[StateLength];

    public void Dispose() => _session.Dispose();
}
