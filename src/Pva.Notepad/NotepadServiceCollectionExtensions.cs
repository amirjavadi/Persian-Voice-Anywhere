using Microsoft.Extensions.DependencyInjection;

namespace Pva.Notepad;

/// <summary>ثبت نوت‌پد در DI.</summary>
public static class NotepadServiceCollectionExtensions
{
    public static IServiceCollection AddNotepad(this IServiceCollection services)
    {
        services.AddSingleton<NotepadSessionStore>();
        services.AddSingleton<NotepadViewModel>();
        return services;
    }
}
