using Microsoft.Extensions.DependencyInjection;
using Pva.Core;

namespace Pva.PersianText;

/// <summary>ثبت پس‌پردازش فارسی در DI.</summary>
public static class PersianTextServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IPersianTextProcessor"/> را ثبت می‌کند. دیکشنری اختیاریِ اصطلاحات
    /// محافظت‌شده (مثلاً «گیت هاب» → «GitHub») برای بهبود ترکیب فارسی/انگلیسی.
    /// </summary>
    public static IServiceCollection AddPersianText(
        this IServiceCollection services,
        IReadOnlyDictionary<string, string>? replacements = null)
    {
        services.AddSingleton<IPersianTextProcessor>(_ => new PersianTextProcessor(replacements));
        return services;
    }
}
