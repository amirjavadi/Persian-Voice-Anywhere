using Microsoft.Extensions.DependencyInjection;

namespace Pva.Storage;

/// <summary>ثبت ذخیره‌سازی در DI.</summary>
public static class StorageServiceCollectionExtensions
{
    /// <summary><see cref="ISettingsStore"/> و مقدار بارگذاری‌شده‌ی <see cref="AppSettings"/> را ثبت می‌کند.</summary>
    public static IServiceCollection AddSettings(this IServiceCollection services, string? path = null)
    {
        var store = new JsonSettingsStore(path);
        services.AddSingleton<ISettingsStore>(store);
        services.AddSingleton(store.Load());
        return services;
    }
}
