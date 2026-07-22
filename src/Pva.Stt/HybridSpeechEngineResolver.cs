using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pva.Core;

namespace Pva.Stt;

/// <summary>یک کاندید موتور STT: نوع، پیکربندی مدل، و سازنده‌ی lazy.</summary>
public sealed record SttCandidate(SpeechEngineKind Kind, ModelConfig Config, Func<ISpeechToTextEngine> Create);

/// <summary>موتور STT مناسب را (با در نظر گرفتن ترجیح و fallback) بارگذاری و برمی‌گرداند.</summary>
public interface ISpeechEngineResolver
{
    Task<ISpeechToTextEngine> ResolveAsync(CancellationToken ct = default);
}

/// <summary>
/// انتخاب هیبرید موتور STT: کاندیدها را به‌ترتیب ترجیح امتحان می‌کند و اولین موتوری که
/// با موفقیت بارگذاری شود را برمی‌گرداند. اگر موتور ترجیحی در دسترس نباشد (مثلاً engine
/// pack نصب نیست)، به‌صورت خودکار به whisper.cpp برمی‌گردد. این منطق خالص است و با
/// موتورهای جعلی کاملاً unit-test می‌شود.
/// </summary>
public sealed class HybridSpeechEngineResolver : ISpeechEngineResolver
{
    private readonly List<SttCandidate> _candidates;
    private readonly ILogger<HybridSpeechEngineResolver> _logger;

    public HybridSpeechEngineResolver(
        IEnumerable<SttCandidate> candidates,
        ILogger<HybridSpeechEngineResolver>? logger = null)
    {
        _candidates = candidates.ToList();
        _logger = logger ?? NullLogger<HybridSpeechEngineResolver>.Instance;

        if (_candidates.Count == 0)
        {
            throw new ArgumentException("حداقل یک کاندید موتور لازم است.", nameof(candidates));
        }
    }

    public async Task<ISpeechToTextEngine> ResolveAsync(CancellationToken ct = default)
    {
        var errors = new List<Exception>();

        foreach (var candidate in _candidates)
        {
            // ساخت موتور هم داخل try است: اگر engine pack نصب نباشد و سازنده استثنا بدهد،
            // باز هم به کاندید بعدی fallback می‌کنیم.
            ISpeechToTextEngine? engine = null;
            try
            {
                engine = candidate.Create();
                await engine.LoadAsync(candidate.Config, ct);
                _logger.LogInformation("موتور STT فعال شد: {Kind}", candidate.Kind);
                return engine;
            }
            catch (OperationCanceledException)
            {
                if (engine is not null)
                {
                    await engine.DisposeAsync();
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "بارگذاری موتور {Kind} ناموفق بود؛ تلاش برای fallback.", candidate.Kind);
                errors.Add(ex);
                if (engine is not null)
                {
                    await engine.DisposeAsync();
                }
            }
        }

        throw new InvalidOperationException(
            "هیچ موتور STT قابل بارگذاری نبود.",
            new AggregateException(errors));
    }
}
