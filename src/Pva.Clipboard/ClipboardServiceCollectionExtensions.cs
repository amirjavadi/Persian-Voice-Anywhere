using Microsoft.Extensions.DependencyInjection;

namespace Pva.Clipboard;

/// <summary>ثبت تاریخچه‌ی کلیپ‌بورد در DI.</summary>
public static class ClipboardServiceCollectionExtensions
{
    public static IServiceCollection AddClipboardHistory(this IServiceCollection services)
    {
        services.AddSingleton<ClipboardHistoryStore>();
        services.AddSingleton<ClipboardHistoryService>();
        services.AddSingleton<ClipboardMonitor>();
        services.AddTransient<ClipboardHistoryViewModel>();
        return services;
    }
}
