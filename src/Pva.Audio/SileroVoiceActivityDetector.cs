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

    private readonly string _modelPath;
    private readonly long _sampleRate;
    private InferenceSession? _session;
    private float[] _state = new float[StateLength];

    public SileroVoiceActivityDetector(string modelPath, int sampleRate = 16000)
    {
        // مدل به‌صورت lazy (هنگام اولین ضبط) بارگذاری می‌شود تا نبودِ فایل مدل،
        // راه‌اندازی برنامه را نشکند و هزینه‌ی Idle نزدیک صفر بماند (طبق CLAUDE.md).
        _modelPath = modelPath;
        _sampleRate = sampleRate;
    }

    private InferenceSession EnsureSession()
    {
        if (_session is not null)
        {
            return _session;
        }

        if (!File.Exists(_modelPath))
        {
            throw new FileNotFoundException(
                $"فایل مدل Silero VAD یافت نشد: {_modelPath}. آن را در پوشه‌ی models/ کنار فایل اجرایی قرار دهید " +
                "(اسکریپت build/fetch-models.ps1 آن را دانلود می‌کند).",
                _modelPath);
        }

        _session = new InferenceSession(_modelPath);
        return _session;
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

        using var results = EnsureSession().Run(inputs);

        // خروجی‌ها به‌ترتیب: [0] احتمال گفتار، [1] حالت جدید.
        var ordered = results.ToList();
        var probability = ordered[0].AsEnumerable<float>().First();
        _state = ordered[1].AsEnumerable<float>().ToArray();

        return probability;
    }

    public void Reset()
    {
        // مدل را همین‌جا (در مسیر شروع ضبط) بارگذاری می‌کنیم تا نبودِ فایل مدل به‌جای
        // یک استثنای هندل‌نشده روی thread پس‌زمینه‌ی صدا، همین‌جا و قابل‌گزارش رخ دهد.
        EnsureSession();
        _state = new float[StateLength];
    }

    public void Dispose() => _session?.Dispose();
}
