using Microsoft.Extensions.DependencyInjection;
using Pva.Core;

namespace Pva.Hotkeys;

/// <summary>ثبت سرویس کلید میانبر سراسری در DI.</summary>
public static class HotkeyServiceCollectionExtensions
{
    public static IServiceCollection AddHotkeys(this IServiceCollection services)
    {
        services.AddSingleton<IHotkeyService, GlobalHotkeyService>();
        return services;
    }
}
