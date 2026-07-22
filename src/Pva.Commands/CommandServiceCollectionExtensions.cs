using Microsoft.Extensions.DependencyInjection;
using Pva.Core;

namespace Pva.Commands;

/// <summary>ثبت تفسیر دستورهای صوتی در DI.</summary>
public static class CommandServiceCollectionExtensions
{
    public static IServiceCollection AddVoiceCommands(this IServiceCollection services)
    {
        services.AddSingleton<ICommandParser, VoiceCommandParser>();
        return services;
    }
}
