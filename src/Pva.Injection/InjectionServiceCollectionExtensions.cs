using Microsoft.Extensions.DependencyInjection;
using Pva.Core;

namespace Pva.Injection;

/// <summary>ثبت تزریق متن در DI.</summary>
public static class InjectionServiceCollectionExtensions
{
    /// <param name="charDelayMs">تأخیر بین کاراکترها (میلی‌ثانیه)؛ 0 یعنی بیشترین سرعت.</param>
    public static IServiceCollection AddTextInjection(this IServiceCollection services, int charDelayMs = 0)
    {
        services.AddSingleton<ITextInjector>(_ => new SendInputTextInjector(charDelayMs));
        return services;
    }
}
